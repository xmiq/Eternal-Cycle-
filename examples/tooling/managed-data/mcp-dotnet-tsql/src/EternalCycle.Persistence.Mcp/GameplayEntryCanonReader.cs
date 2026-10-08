using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

// Exact owner reads only. The reference projection is deliberately not a Canon
// graph loader or an inference engine; FR-028 owns scalable closure discovery.
public sealed class GameplayEntryCanonReader(ICampaignPersistenceStore persistence,
    IOptions<GameplayEntryOptions> options) : IGameplayEntryCanonReader
{
    internal const int MaximumReads = 32;
    internal const int MaximumCanonBytes = 65_536;
    private static readonly EntryCanonRole[] RequiredRoles = [EntryCanonRole.PlayerState, EntryCanonRole.Controller, EntryCanonRole.Protagonist,
        EntryCanonRole.Incarnation, EntryCanonRole.Time, EntryCanonRole.Scene, EntryCanonRole.Placement];

    public async Task<GameplayEntryCanonContext> ReadAsync(PreparedGameplayEntry entry, CancellationToken token)
    {
        var evidence = new List<GameplayEntryReadEvidence>();
        try
        {
            if (!options.Value.CurrentSessions.TryGetValue(entry.CampaignId, out var session) || !ValidAddress(session))
                throw Missing(EntryCanonRole.CurrentSession, null, EntryCanonStatus.Unavailable);
            var root = await ReadAsync(session, EntryCanonRole.CurrentSession, null);
            GameplayEntrySessionPlan plan;
            try { plan = JsonSerializer.Deserialize<GameplayEntrySessionPlan>(root.PayloadJson, SqlServerPlayerInteractionStore.Json)!; }
            catch (JsonException error) { throw new EntryCanonException("GAMEPLAY_ENTRY_CANON_INCOMPLETE", evidence, error); }
            if (plan is null || plan.InputSha256 != entry.InputSha256)
                throw new EntryCanonException("GAMEPLAY_ENTRY_CANON_STALE", evidence);
            if (!PlayerInteractionService.Bounded(plan.CampaignMode, 128) || plan.Reads.Count > MaximumReads - 1 ||
                plan.Reads.Any(read => read.Role == EntryCanonRole.CurrentSession || !Enum.IsDefined(read.Role) || !Enum.IsDefined(read.Visibility)) ||
                RequiredRoles.Any(role => plan.Reads.Count(read => read.Role == role) != 1) ||
                plan.Reads.Where(read => read.Address is not null).Select(read => read.Address).Append(session).Distinct().Count() !=
                    plan.Reads.Count(read => read.Address is not null) + 1)
                throw new EntryCanonException("GAMEPLAY_ENTRY_CANON_INCOMPLETE", evidence);
            // Freeze and validate selectors through the existing request contract,
            // not a new normalization system. These signals are canonical, not NLP.
            var selectors = CompiledRuleRetrieval.Prepare(new(new(entry.Baseline.RulesetId,
                entry.Baseline.RuleRepresentation == "CompiledArtifact" ? entry.Baseline.ActiveRuleIdentity! : new string('A', 64)),
                "gameplay.resolve", plan.CampaignMode, plan.QueryTerms)
            {
                ModuleIds = plan.ModuleIds, RequiredRuleSourceIds = plan.RequiredRuleSourceIds,
                RequiredSnippetIds = plan.RequiredSnippetIds, WorldModelId = entry.Baseline.WorldModelId
            }, token);
            plan = plan with { QueryTerms = selectors.QueryTerms, ModuleIds = selectors.ModuleIds,
                RequiredRuleSourceIds = selectors.RequiredRuleSourceIds, RequiredSnippetIds = selectors.RequiredSnippetIds };
            var records = new List<CanonicalRecord> { root };
            var bytes = Encoding.UTF8.GetByteCount(root.PayloadJson);
            foreach (var read in plan.Reads)
            {
                token.ThrowIfCancellationRequested();
                if (read.Visibility == EntryCanonVisibility.Restricted)
                    throw Missing(read.Role, read.Address, EntryCanonStatus.Unavailable);
                if (read.LegitimatelyAbsent)
                {
                    if (read.Address is not null || !PlayerInteractionService.Bounded(read.AbsenceReason, 256) || read.ExpectedRevision is not null)
                        throw Missing(read.Role, read.Address, EntryCanonStatus.Incomplete);
                    evidence.Add(new(read.Role, null, null, null, EntryCanonStatus.LegitimatelyAbsent, read.Visibility, read.AbsenceReason));
                    continue;
                }
                if (!ValidAddress(read.Address)) throw Missing(read.Role, read.Address, EntryCanonStatus.Incomplete);
                var record = await ReadAsync(read.Address!, read.Role, read.ExpectedRevision, read.Visibility);
                bytes += Encoding.UTF8.GetByteCount(record.PayloadJson);
                if (bytes > MaximumCanonBytes) throw Missing(read.Role, read.Address, EntryCanonStatus.Incomplete);
                records.Add(record);
            }
            return new(plan with { Reads = Array.AsReadOnly(plan.Reads.ToArray()) }, records.AsReadOnly(), evidence.AsReadOnly());
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is not EntryCanonException and not CompiledRuleRetrievalException and not OutOfMemoryException)
        {
            evidence.Add(new(EntryCanonRole.CurrentSession, null, null, null, EntryCanonStatus.Failed, EntryCanonVisibility.GmAuthorized));
            throw new EntryCanonException("GAMEPLAY_ENTRY_CANON_FAILED", evidence.AsReadOnly(), error);
        }

        async Task<CanonicalRecord> ReadAsync(RecordAddress address, EntryCanonRole role, long? revision,
            EntryCanonVisibility visibility = EntryCanonVisibility.GmAuthorized)
        {
            var result = await persistence.ReadRecordsAsync(entry.CampaignId, [address], token);
            if (result.CampaignVersion != entry.Baseline.CampaignVersion) throw Missing(role, address, EntryCanonStatus.Stale);
            var record = result.Records.SingleOrDefault(value => value.OwnerDomain == address.OwnerDomain && value.RecordId == address.RecordId);
            if (record is null || record.Tombstone) throw Missing(role, address, EntryCanonStatus.Unavailable);
            if (record.CampaignVersion > entry.Baseline.CampaignVersion || revision is not null && record.RecordRevision != revision)
                throw Missing(role, address, EntryCanonStatus.Stale);
            if (Encoding.UTF8.GetByteCount(record.PayloadJson) > MaximumCanonBytes)
                throw Missing(role, address, EntryCanonStatus.Incomplete);
            evidence.Add(new(role, address, record.RecordRevision, Hash(record.PayloadJson), EntryCanonStatus.Completed, visibility));
            return record;
        }

        EntryCanonException Missing(EntryCanonRole role, RecordAddress? address, EntryCanonStatus status)
        {
            evidence.Add(new(role, address, null, null, status, EntryCanonVisibility.GmAuthorized));
            return new("GAMEPLAY_ENTRY_CANON_" + (status == EntryCanonStatus.Stale ? "STALE" : status == EntryCanonStatus.Incomplete ? "INCOMPLETE" : "UNAVAILABLE"), evidence.AsReadOnly());
        }
    }

    internal static bool ValidAddress(RecordAddress? address) => address is not null &&
        PlayerInteractionService.Bounded(address.OwnerDomain, 128) && PlayerInteractionService.Bounded(address.RecordId, 128);

    internal static bool CompleteEvidence(GameplayEntryCanonContext context)
    {
        var reads = context.Evidence;
        if (reads.Count > MaximumReads || reads.Count(read => read.Role == EntryCanonRole.CurrentSession && read.Status == EntryCanonStatus.Completed) != 1 ||
            RequiredRoles.Any(role => reads.Count(read => read.Role == role) != 1) ||
            context.Records.Sum(record => (long)Encoding.UTF8.GetByteCount(record.PayloadJson)) > MaximumCanonBytes)
            return false;
        var completed = 0;
        var addresses = new HashSet<RecordAddress>();
        var records = new Dictionary<RecordAddress, CanonicalRecord>();
        foreach (var record in context.Records)
            if (!records.TryAdd(new(record.OwnerDomain, record.RecordId), record)) return false;
        foreach (var read in reads)
        {
            if (!Enum.IsDefined(read.Role) || !Enum.IsDefined(read.Visibility) || read.Visibility == EntryCanonVisibility.Restricted) return false;
            if (read.Status == EntryCanonStatus.LegitimatelyAbsent && read.Address is null && read.Revision is null && read.PayloadSha256 is null &&
                PlayerInteractionService.Bounded(read.AbsenceReason, 256)) continue;
            if (read.Status != EntryCanonStatus.Completed || !ValidAddress(read.Address) || !addresses.Add(read.Address!)) return false;
            if (!records.TryGetValue(read.Address!, out var record) || record.Tombstone || record.RecordRevision != read.Revision || Hash(record.PayloadJson) != read.PayloadSha256) return false;
            completed++;
        }
        return completed == context.Records.Count;
    }

    internal static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
