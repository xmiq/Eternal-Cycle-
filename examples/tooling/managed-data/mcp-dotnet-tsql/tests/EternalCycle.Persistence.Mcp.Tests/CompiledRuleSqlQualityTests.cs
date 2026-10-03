using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient.Diagnostics;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

// SqlClient 7 exposes typed DiagnosticSource payloads. Observe only this
// disposable fixture's commands, without changing the store or keeping a
// connection string, timing, SQL output, campaign data or process identity.
internal sealed class SqlQualityReadObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
{
    internal sealed record Read(string Sql, string? ImportId, string? RulesetId, string? SemanticSha256);
    private readonly string database;
    private readonly List<IDisposable> subscriptions = [];
    private readonly List<Read> reads = [];
    private readonly IDisposable all;
    private bool overflow;

    internal SqlQualityReadObserver(string database)
    {
        this.database = database;
        all = DiagnosticListener.AllListeners.Subscribe(this);
    }

    public void OnNext(DiagnosticListener listener)
    {
        lock (reads) subscriptions.Add(listener.Subscribe(this, name => name == SqlClientCommandBefore.Name));
    }

    public void OnNext(KeyValuePair<string, object?> item)
    {
        if (item.Value is not SqlClientCommandBefore before || before.Command.Connection?.Database != database) return;
        var command = before.Command;
        string? Parameter(string name) => command.Parameters.Contains(name) ? command.Parameters[name].Value as string : null;
        lock (reads)
        {
            if (reads.Count >= 16) { overflow = true; return; }
            reads.Add(new(command.CommandText, Parameter("@id"), Parameter("@ruleset"), Parameter("@semantic")));
        }
    }

    internal Read[] Drain()
    {
        lock (reads)
        {
            Assert.False(overflow, "Retrieval unexpectedly exceeded the bounded command observation window.");
            var snapshot = reads.ToArray();
            reads.Clear();
            return snapshot;
        }
    }

    public void OnError(Exception error) => throw new InvalidOperationException("SQL evidence subscription failed.");
    public void OnCompleted() { }
    public void Dispose()
    {
        all.Dispose();
        lock (reads) foreach (var subscription in subscriptions) subscription.Dispose();
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task G_AllCanonicalQualityFixturesMatchSQLWithBoundedArtifactLocalReads()
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Canonical());
        var service = await ReadyAsync(approved);
        var importId = CompiledRulesArtifactImport.CreateImportId(approved.Artifact);
        // A second active artifact with different content/size makes accidental
        // whole-store/history loading observable rather than vacuously isolated.
        var unrelated = await CompiledRulesImportFixtures.Approve(GraphBytes("diamond"));
        _ = await ReadyAsync(unrelated);
        Assert.Equal(2, await CountAsync("imported_rule_artifacts"));
        var observations = new List<object>();
        using (var observer = new SqlQualityReadObserver(database))
        {
            foreach (var fixture in CompiledRuleQualityEvidence.Cases().OrderBy(item => item.Id, StringComparer.Ordinal))
            {
                var expected = CompiledRuleQualityEvidence.Measure(approved.Artifact, fixture);
                string projection;
                if (expected.Result is { } packet)
                {
                    var actual = await service.GetContextAsync(expected.Request, default);
                    projection = CompiledRuleEquivalence.Project(actual);
                    Assert.Equal(CompiledRuleEquivalence.Project(packet), projection);
                }
                else
                {
                    var error = await Assert.ThrowsAsync<CompiledRuleRetrievalException>(() => service.GetContextAsync(expected.Request, default));
                    Assert.Equal(expected.Failure, error.Failure);
                    projection = error.Code; // Stable failure, never raw driver text.
                }
                var commands = observer.Drain();
                Assert.InRange(commands.Length, 6, 8); // Observational bound, not a normative six-command ABI.
                var artifactReads = commands.Where(read => read.Sql.Contains("imported_rule_", StringComparison.Ordinal) || read.Sql.Contains("published_rule_artifacts", StringComparison.Ordinal)).ToArray();
                Assert.InRange(artifactReads.Length, 6, 8); // Record, rather than mandate, the current six-read strategy.
                Assert.All(artifactReads, read => Assert.StartsWith("SELECT", read.Sql.TrimStart(), StringComparison.Ordinal));
                Assert.Contains(artifactReads, read => read.RulesetId == approved.Artifact.Ruleset.RulesetId && read.SemanticSha256 == approved.SemanticSha256);
                Assert.All(artifactReads.Where(read => read.ImportId is not null), read => Assert.Equal(importId, read.ImportId));
                foreach (var table in new[] { "imported_rule_sources", "imported_rule_snippets", "imported_rule_dependencies" })
                {
                    var read = Assert.Single(artifactReads, read => read.Sql.Contains(table, StringComparison.Ordinal));
                    Assert.Contains("WHERE import_id = @id ORDER BY", read.Sql, StringComparison.Ordinal);
                    Assert.Equal(importId, read.ImportId);
                }
                observations.Add(new { fixture.Id, SqlCommands = commands.Length, ArtifactReadCommands = artifactReads.Length,
                    VerifiedMaterialized = new { Headers = 1, Sources = approved.Artifact.RuleSources.Count, Snippets = approved.Artifact.Snippets.Count,
                        DependencyRows = approved.Artifact.RuleSources.Sum(source => source.DependencyRuleSourceIds.Count),
                        RetrievalAssociations = approved.Artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count), ApprovedBytes = approved.CopyBytes().Length },
                    ResultProjectionSha256 = CompiledRuleQualityEvidence.Hash(System.Text.Encoding.UTF8.GetBytes(projection)),
                    Failure = expected.Failure?.ToString() });
                // Observer does not feed back into execution. The same durable
                // request is compared again with observation disabled below.
            }
        }
        var sample = CompiledRuleQualityEvidence.Measure(approved.Artifact, CompiledRuleQualityEvidence.Cases().Single(item => item.Id == "combat-fighting-full"));
        Assert.Equal(CompiledRuleEquivalence.Project(sample.Result!), CompiledRuleEquivalence.Project(await service.GetContextAsync(sample.Request, default)));
        var report = JsonSerializer.SerializeToUtf8Bytes(new { ReportFormatVersion = 1, approved.SemanticSha256,
            Method = "Typed SqlClient command observation; verified materialized records, not wire-byte or physical page counters.", Fixtures = observations },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var destination = Environment.GetEnvironmentVariable("ETERNAL_CYCLE_WRITE_SQL_QUALITY_REPORT");
        if (destination is null) Assert.Equal(File.ReadAllBytes(CompiledRuleQualityEvidence.FixturePath("expected-sql-quality-report.json")), report);
        else File.WriteAllBytes(destination, report);
        Assert.Equal(2, await CountAsync("imported_rule_artifacts"));
    }

    [SqlImportTheory]
    [InlineData("direct")]
    [InlineData("chain")]
    [InlineData("shared")]
    [InlineData("diamond")]
    [InlineData("root-dependency")]
    public async Task G_SyntheticDependencyCostsRemainWholeAndSQLIdentical(string graph)
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes(graph));
        var service = await ReadyAsync(approved);
        var full = CompiledRuleEquivalence.Reference(approved.Artifact, Query(approved.Artifact, "select"));
        foreach (var budget in new[] { 8000, full.Totals.UsedEstimatedTokens, full.Totals.UsedEstimatedTokens - 1 })
        {
            using var observer = new SqlQualityReadObserver(database);
            var request = Query(approved.Artifact, "select", budget);
            var reference = CompiledRuleEquivalence.Reference(approved.Artifact, request);
            Assert.Equal(CompiledRuleEquivalence.Project(reference), CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
            Assert.InRange(observer.Drain().Length, 6, 8);
            foreach (var decision in reference.Decisions.Where(decision => decision.Selected))
            {
                var group = reference.Input.Roots.Single(group => group.Root.SnippetId == decision.Root.SnippetId);
                foreach (var prerequisite in group.Prerequisites) Assert.Contains(reference.Members, member => member.SnippetId == prerequisite.SnippetId);
            }
            Assert.Equal(reference.Members.Sum(member => member.Member.Snippet.EstimatedTokens), reference.Totals.UsedEstimatedTokens);
        }
    }

    [SqlImportTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task G_SyntheticTiesPreparationAndAlwaysIncludeKeepTheirSeparateMeanings(bool unequalPriority)
    {
        var bytes = CompiledRulesImportFixtures.Change(GraphBytes("shared"), node =>
        {
            node["ruleSources"]![0]!["applicability"]!["priority"] = unequalPriority ? 10 : 0;
            node["ruleSources"]![1]!["applicability"]!["priority"] = unequalPriority ? 20 : 0;
            node["ruleSources"]![0]!["applicability"]!["preparationTier"] = "OptionalRare";
            node["ruleSources"]![1]!["applicability"]!["preparationTier"] = "ImmediateGameplayCore";
            node["ruleSources"]![2]!["applicability"]!["alwaysInclude"] = true;
        });
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, "select");
        var reference = CompiledRuleEquivalence.Reference(approved.Artifact, request);
        Assert.Equal(unequalPriority ? new[] { "b", "a" } : new[] { "a", "b" }, reference.Input.Input.Candidates.Take(2).Select(root => root.SnippetId));
        Assert.All(reference.Input.Input.Candidates.Take(2), root => Assert.Equal(900, root.VocabularyScore));
        var always = Assert.Single(reference.Decisions, decision => decision.Root.SnippetId == "c");
        Assert.True(always.Required);
        Assert.Equal(CompiledRuleCandidateReason.AlwaysInclude, always.Root.Reasons);
        Assert.Equal(0, always.Root.VocabularyScore);
        Assert.Equal(CompiledRuleEquivalence.Project(reference), CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(CompiledRuleEquivalence.Project(reference), CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
        }
        finally { CultureInfo.CurrentCulture = prior; }
    }

    [SqlImportFact]
    public async Task G_UnicodeNfcIdentityAndDuplicateConceptWeightsSurviveSQL()
    {
        var bytes = CompiledRulesImportFixtures.Change(GraphBytes("direct"), node =>
            node["snippets"]![0]!["retrieval"]!["terms"] = new JsonArray(
                new JsonObject { ["term"] = "caf\u00e9", ["kind"] = "first-concept", ["weight"] = 700 },
                new JsonObject { ["term"] = "caf\u00e9", ["kind"] = "second-concept", ["weight"] = 600 }));
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var service = await ReadyAsync(approved);
        var expected = CompiledRuleEquivalence.Reference(approved.Artifact, Query(approved.Artifact, "caf\u00e9"));
        var request = Query(approved.Artifact, " CAFE\u0301 |caf\u00e9|CAF\u00c9");
        var actual = await service.GetContextAsync(request, default);
        Assert.Equal(CompiledRuleEquivalence.Project(expected), CompiledRuleEquivalence.Project(actual));
        var root = Assert.Single(actual.Input.Input.Candidates, root => root.SnippetId == "a");
        Assert.Equal(1300, root.VocabularyScore);
        Assert.Equal(new[] { "first-concept", "second-concept" }, root.Matches.Select(term => term.Kind));
    }
}
