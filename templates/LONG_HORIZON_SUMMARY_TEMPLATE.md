# Long-Horizon Summary Template

Use this blank storage-neutral template for one established historical interval. Populated summaries belong in external Campaign State.

## Document Control

- **Template owner:** [Long-Horizon Simulation Summaries](../docs/persistence/LONG_HORIZON_SUMMARIES.md)
- **Dependencies:** stable period, event, scope, Entity, Life, and source-owner identities
- **Optional fields:** every category not material to the interval
- **Consumers:** Time Skips, Reincarnation gaps, Age transitions, World Resets, historical retrieval, migration, validation, and filtered presentation

## Usage Guidance

1. Resolve meaningful changes through specialist owners before summarizing.
2. Preserve concise established causal chains and honest unknowns.
3. Reference current-state owners rather than copying mutable values.
4. Keep Life Summaries separate.
5. Filter player presentation through Perspective and secrecy boundaries.

## Historical Period

- **Historical Period ID:** `<stable opaque ID>`
- **Status/version:** `<open, closed, or superseded / positive version>`
- **Scope type:** `<Local Interval, Period Summary, Era Summary, Cross-Domain Summary, or established extension>`
- **Scope references:** `<world, Contact Domain, region, location, Project, theme, or other stable references>`
- **Start marker:** `<date, Age, event, relative marker, Unknown, or Estimated>`
- **End marker:** `<date, Age, event, relative marker, Unknown, or Estimated>`
- **Temporal precision:** `<exact, bounded, approximate, relative, or Unknown>`
- **Compression trigger:** `<Time Skip, Interlife gap, Age transition, World Reset, migration, simulation interval, or other authorized trigger>`
- **Provenance:** `<source records, Transaction ID, Migration ID, and authority>`

## What Meaningfully Changed

`<concise established changes; omit immaterial categories>`

## Causal Links

| Cause/reference | Material transition | Outcome/reference | Certainty |
|---|---|---|---|
| `<stable source>` | `<concise supported link>` | `<stable result>` | `<established, disputed, inferred, Estimated, or Unknown>` |

## Surviving Continuities

- `<stable continuity hook and target reference>`

## What Was Lost or Transformed

- `<established loss/transformation and owner/event reference>`

## Unresolved Threads and Unknowns

- `<thread/reference/status: Unknown, Not Established, Estimated, or Source Recovery Required>`

## Important References

| Type | Stable ID | Historical role | Visibility |
|---|---|---|---|
| `<Entity, Life, event, Relationship, faction, species, Project, Infrastructure, Autonomous, Gate, summary, or extension>` | `<ID>` | `<participant, cause, consequence, continuity, unresolved thread, nested period, or other role>` | `<GM, player, character, or protected>` |

## Nested Summary Links

- **Parent periods:** `<stable IDs; optional>`
- **Child periods:** `<stable IDs; optional>`
- **Rollup role:** `<scope relation; optional>`

## Revision Record

- **Previous version:** `<version or none>`
- **Reason:** `<factual correction, source recovery, retcon, migration, or initial finalization>`
- **Authority:** `<authorized source>`
- **Changed references:** `<stable IDs>`

## Derived Historical Transition

This section is player-facing and non-authoritative.

### Time Passes — `<honest duration or transition label>`

**Major Changes**

- `<player-visible established change>`

**What Survived**

- `<player-visible continuity>`

**What Was Lost**

- `<player-visible loss>`

**Old Threads Still Relevant**

- `<player-visible unresolved hook>`

**What the Current Character Can Know**

- `<Perspective-filtered Character Knowledge; do not infer from player access>`

## Validation Notes

- [ ] Historical Period ID is unique and stable.
- [ ] Temporal ordering is coherent at the claimed precision.
- [ ] Scope and all structured references resolve or have explicit Record Gaps.
- [ ] Important established causal links survive compression.
- [ ] Missing history remains unknown rather than completed by invention.
- [ ] Mutable current state remains with specialist owners.
- [ ] Life Summary and Long-Horizon Summary roles remain separate.
- [ ] Nested period references are acyclic.
- [ ] Closed-period changes have authorized revision provenance.
- [ ] Player-facing content exposes no GM Secret or unsupported Character Knowledge.

## Cross-References

- [Long-Horizon Simulation Summaries](../docs/persistence/LONG_HORIZON_SUMMARIES.md)
- [Life Archive Template](LIFE_ARCHIVE_TEMPLATE.md)
- [Timeline Event Template](TIMELINE_EVENT_TEMPLATE.md)
- [Campaign History Template](CAMPAIGN_HISTORY_TEMPLATE.md)
- [World-State Template](WORLD_STATE_TEMPLATE.md)
- [Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
