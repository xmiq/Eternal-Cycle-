# Google Drive Persistence Adapter

## Purpose

This Persistence Adapter defines how a ChatGPT runtime uses Google Drive for canonical campaign-file deployment and backup while preserving the storage-neutral [Campaign Persistence Engine](../../../persistence/README.md).

It contains no campaign-specific Drive identifier, URL, credential, or populated state. Google Drive implements remote identity and transport behavior; it does not adjudicate gameplay or own campaign meaning.

## Document Control

- **Owner:** this adapter owns exact remote identity, fetch-latest, canonical replacement, remote read-back, backup propagation, folder hygiene, remote concurrency, and deployment recovery
- **Primary authorities:** [AI Runtime Model](../../AI_RUNTIME_MODEL.md), [AI Save Protocol](../../AI_SAVE_PROTOCOL.md), [Save Update Protocol](../../../persistence/SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](../../../persistence/PERSISTENCE_VALIDATION.md)
- **Dependencies:** external Campaign Configuration, authorized Drive access, an identified canonical file, selected logical-store adapter, and backup policy
- **Extensions:** logical-store adapters may produce validated candidate bytes for deployment without taking over remote identity
- **Consumers:** ChatGPT execution profile, Adapter Chains, campaign custodians, save operators, and recovery tools
- **Repository boundary:** no campaign identifier, Drive file or folder identifier, URL, sharing principal, credential, token, current filename, or populated record belongs here

## Adapter Role

The Google Drive adapter owns:

- locating the configured remote file by exact observed identity;
- fetching the latest bytes and metadata;
- protecting against stale remote writes;
- replacing the existing canonical file rather than creating a competitor;
- refetching and comparing the canonical result;
- propagating and verifying required backups;
- preserving configured sharing, ownership, and folder organization;
- reporting remote failure and recovery evidence.

It does not own:

- gameplay adjudication;
- Campaign Persistence meaning or authority;
- SQLite or other logical-store transactions;
- semantic interpretation of campaign records;
- player-facing narration.

## Campaign Configuration Boundary

Campaign Configuration provides authorized deployment values such as:

- canonical file or folder identity;
- expected canonical filename where needed;
- selected logical-store adapter;
- backup or snapshot destination;
- backup policy and naming convention;
- required comparison methods;
- sharing and ownership constraints;
- concurrency or recovery policy.

Use only identifiers and locations actually observed through authorized configuration and tool results. Never invent an ID, URL, path, permission, or backup destination.

These values remain outside this universal adapter document.

## Exact Remote Identity

One configured Drive file has canonical deployment authority. Its role follows the campaign's Save Index and configuration, not filename similarity or recency alone.

The adapter must:

- preserve the canonical file identity when replacing content;
- distinguish the canonical file from backups, exports, caches, and handoff documents;
- reject ambiguous duplicate candidates;
- avoid creating a new canonical file on every save;
- preserve stable authority across ordinary content replacement.

A downloaded copy is a Local Working Copy and is non-authoritative until freshness is re-established.

## Fetch-Latest Procedure

Before dependent gameplay or a write:

1. inspect the configured canonical Drive location;
2. resolve the exact observed canonical file identity;
3. read current metadata, version evidence, and configured concurrency evidence;
4. download the latest bytes;
5. compare the retrieved identity with Campaign Configuration and the active Save Index;
6. pass the bytes to the selected logical-store adapter;
7. discard or label older cached copies as non-authoritative.

If the configured file cannot be found uniquely and safely, stop. Do not choose the most recent similar filename.

A missing Local Working Copy does not bypass this procedure and does not authorize creation of a blank canonical database. Report the canonical target missing only after the configured exact-identity lookup or authorized recovery search fails.

## Candidate Input

Google Drive receives candidate bytes only after the selected logical-store adapter has:

- applied the complete Affected Set;
- completed its transaction;
- run its implementation-specific checks;
- reopened and validated the candidate as required;
- supplied expected identity, parent version, hash, or equivalent comparison evidence.

Drive does not repair or reinterpret candidate contents.

## Canonical Replacement Procedure

When Campaign Configuration makes Google Drive part of the authoritative adapter chain, an FR-011 state-changing turn remains unclosed until required remote replacement and read-back succeed. A local SQLite commit alone does not satisfy that configured boundary.

Before upload, recheck that the remote canonical identity and parent evidence still match what was fetched.

Then:

1. replace the bytes of the existing canonical Drive file;
2. preserve its file identity, configured location, and canonical role;
3. preserve sharing and ownership unless an authorized configuration change says otherwise;
4. confirm the write request completed;
5. refetch the same canonical file by exact identity;
6. compare the refetched result with the validated candidate;
7. pass the refetched bytes through required logical-store and persistence validation.

Creating another file with a similar name does not activate a Save Point.

## Remote Read-Back Validation

A successful upload response is insufficient. The adapter must retrieve the destination again and verify, where available:

- exact remote file identity;
- expected parent and candidate version relationship;
- byte length;
- cryptographic hash;
- exact byte equality;
- logical-store semantic validation;
- expected-versus-actual Affected Set;
- active Save Index and Campaign Version.

Matching size alone is insufficient. Matching bytes do not replace semantic validation.

Only successful required remote read-back and comparison may support `☁️💾`. Upload attempted, upload accepted, synchronization requested, local candidate present, and metadata-only ambiguity cannot.

## Backup Propagation

After canonical replacement and canonical read-back pass:

1. use the exact verified canonical bytes;
2. identify the configured backup or snapshot destination;
3. create or replace the required backup according to policy;
4. preserve the configured folder and naming convention;
5. refetch the backup by exact observed identity;
6. compare it with the verified canonical candidate;
7. record operational completion through the authorized external mechanism if policy requires it.

The strict ChatGPT Save-Before-Delivery operation is complete only after every backup step required by Campaign Configuration passes.

A backup is not a competing active campaign authority. Recovery promotes it only through the configured recovery, continuity, or migration procedure.

## Comparison Requirements

Use the strongest comparisons available and required by Campaign Configuration:

- exact file identity;
- parent and resulting Campaign Version;
- byte length;
- cryptographic hash;
- exact byte comparison;
- logical-store integrity;
- semantic persistence validation;
- expected-versus-actual state comparison.

Record which comparisons actually ran. Do not claim cryptographic or exact verification when the available tool exposed only metadata.

## Standard Adapter Order

A typical remote SQLite deployment follows:

```text
fetch latest Google Drive canonical file
  -> validate and transact through SQLite
  -> validate SQLite candidate read-only
  -> replace Google Drive canonical file
  -> refetch and validate canonical bytes
  -> propagate Google Drive backup
  -> refetch and validate backup bytes
  -> permit durable narration
```

Alternative promotion or backup strategies must be explicit in Campaign Configuration and preserve the same authority, atomicity, recovery, and validation guarantees.

## Staleness and Concurrency

Compare the remote identity, version, modified metadata, hash, or configured concurrency token immediately before replacement.

If the remote canonical file changed after fetch:

- do not overwrite blindly;
- preserve the pending Transaction ID and candidate as non-authoritative;
- fetch the new canonical source;
- classify the branch under the Save Update Protocol;
- reconcile, rerun, or reject through the proper owner;
- publish only a validated descendant.

Last-writer-wins is forbidden.

## Failure Handling

### Fetch failure

If the latest canonical file cannot be fetched:

- do not fall back to an old Local Working Copy for authoritative gameplay;
- report source recovery or Operational Failure;
- preserve the last known valid authority without pretending it was reloaded.

### Canonical write failure

If replacement fails:

- do not claim the candidate is active;
- preserve the previous remote canonical file where possible;
- inspect actual remote state before retry;
- keep dependent durable narration blocked.
- preserve any validated local candidate for idempotent cloud retry without rerunning gameplay writes;
- report `⚠️`, not local success, when Google Drive is canonical authority.

### Canonical read-back failure

If replacement appears successful but canonical read-back cannot verify it:

- treat the remote state as uncertain;
- stop dependent play;
- inspect exact remote identity and bytes;
- recover idempotently from the existing Transaction ID.
- never display `☁️💾` without successful verification.

### Backup failure

If canonical read-back passes but a required backup write or read-back fails:

- report the strict save operation as incomplete;
- preserve the verified canonical result;
- follow Campaign Configuration's authorized recovery policy;
- do not falsely claim full success or silently waive the backup requirement.

## Folder Hygiene

A canonical deployment should normally maintain clearly distinct roles for:

- one canonical save artifact;
- an optional handoff or start document;
- one configured backup or snapshot area;
- explicitly approved supporting configuration.

Avoid duplicate canonical databases, stale exports in the canonical location, ambiguous filenames, obsolete handoff documents, and backups mixed with the active artifact.

Folder organization aids operation but does not create campaign authority by itself.

## Security Sharing and Ownership

Preserve existing sharing and ownership unless an authorized configuration change explicitly requires otherwise.

Never expose:

- authenticated download URLs;
- bearer material;
- access tokens;
- connector-internal references;
- private file or folder identifiers;
- unauthorized sharing principals.

Use least-necessary access. A successful content update does not authorize permission changes.

## Adapter Composition

When Google Drive carries a SQLite store:

- SQLite owns database transaction and integrity behavior;
- Google Drive owns remote canonical identity, fetch, replacement, remote read-back, backup propagation, and backup read-back;
- Campaign Configuration owns selection and deployment-specific values;
- the Campaign Persistence Engine owns logical continuity, authority, and validation semantics.

Google Drive may also deploy another supported logical format. Replacing the inner adapter must not change remote identity rules or Campaign Persistence meaning.

### Living Codex Specialization

Google Drive may deploy the separately configured [GM Living Codex SQLite database](../../../gm-living-codex/PERSISTENCE_MODEL.md). That deployment uses its own exact canonical identity, current verified backup, dated recovery snapshots, migration manifests, concurrency evidence, canonical read-back, and backup read-back. It remains separate from every campaign canonical file and backup area.

In this specialization, the Living Codex persistence model owns reusable-design meaning and full-save completion while Google Drive owns remote transport and identity. The adapter must not treat a Codex file as Campaign State, place campaign facts in it, or infer campaign adoption from successful deployment.

## Safeguards

- Exact observed remote identity is required.
- The latest source is fetched before dependent play and writes.
- Cached files are non-authoritative.
- Canonical updates replace the existing file rather than create competitors.
- Canonical and required backup destinations receive Read-Back Validation.
- Exact or cryptographic comparison is used where available and reported honestly.
- Stale writes and last-writer-wins are rejected.
- Sharing, ownership, identifiers, and credentials remain protected.
- Google Drive never adjudicates gameplay.
- No campaign-specific Drive value or populated state belongs here.

## Related Documents

- [ChatGPT GM Universal Instructions](../CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md)
- [SQLite Persistence Adapter](SQLITE_PERSISTENCE_ADAPTER.md)
- [GM Living Codex Persistence Model](../../../gm-living-codex/PERSISTENCE_MODEL.md)
- [AI Runtime Model](../../AI_RUNTIME_MODEL.md)
- [AI Capabilities and Limitations](../../AI_CAPABILITIES_AND_LIMITATIONS.md)
- [AI Save Protocol](../../AI_SAVE_PROTOCOL.md)
- [Campaign State Model](../../../persistence/CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](../../../persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../../../persistence/PERSISTENCE_VALIDATION.md)
- [Canonical Terminology](../../../../design/TERMINOLOGY.md)
