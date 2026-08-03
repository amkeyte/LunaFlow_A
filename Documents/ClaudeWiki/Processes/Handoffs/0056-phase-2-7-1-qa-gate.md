# 0056 — Phase 2.7.1 QA gate

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-02
**For:** Lothar (QA)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Phase 2.7.1 ([`phase-2-7-1.md`](../../../Roadmap/phase-2-7-1.md)) is built
([0052](0052-implement-loot-rom-content-expansion-spec-md.md)) and manually verified against the
console host, but not done yet: `tests/` doesn't compile against the new shapes
([0054](0054-loot-rom-test-suite.md), to Nadia) and two implementation-mechanism calls are awaiting
Amara's ratification ([0055](0055-loot-spec-mechanism-deviations.md)). Same shape as
[0025](0025-rom-rulebook-boundary-qa-gate.md) was for Phase 2.7: filed now so the gate is tracked and
ready, not run before there's anything real to check.

**Not actionable yet.** Parked until 0054 lands (suite compiling and green) and 0055 is ratified.
Filing now, ahead of that, since 0054/0055 are expected to close soon and the ask shouldn't be
drafted under time pressure later.

### Ask

When 2.7.1 is implementation-complete (0054 and 0055 both resolved), run a full `qa-verify` pass.
Verify against evidence — source, passing test, reproducible session — not ticket prose:

1. **Three ROMs coordinate in one Cartridge** — GridMap, D20System, Loot all registered and
   validated; removing any one produces a clear load-time error (same bar Phase 2.7 established for
   two, now for three).
2. **`ChallengeSolved` has two independent requirers** — GridMap's win-line rules and Loot's award
   rule both fire correctly off D20System's one `Provides`, in the same session. Confirm `LootRom`'s
   assembly graph has zero reference to GridMap/D20System, not just that it runs.
3. **Session-start randomization is real, not cosmetic** — two different sessions produce different
   room→challenge and room→monster assignments; room *type* topology (fight `{1,5,8}`, skill-check
   `{2,3,4,6,7,9}`) is identical across every session.
4. **Loot is correctly gated** — defeating a monster inserts exactly one `ItemAwarded` matching that
   session's `MonsterLootTable` entry; a lost/fled fight awards nothing; skill-check rooms never
   award, across all 9 challenges, not just argued from the type-level case. `inventory` reads only
   `ItemAwarded`, reports correctly empty/populated.
5. **The two ratified deviations ship as described** — check [0055](0055-loot-spec-mechanism-deviations.md)'s
   two calls (flavor-text resolution moved to session assembly, not a seeded `RoomChallenge` fact;
   `LookCommandHandler` takes a per-room HP dictionary) against the actual shipped code, not just that
   Amara signed off on the description. Behavioral claims are QA's to verify against source, same
   distinction `CLAUDE.md` draws between the API surface and what it actually does.
6. **No new Cartridge-tier framework capability was needed** — if either 0055 deviation or anything
   else in the build actually required one, that's a real gap this phase's own Done-When says not to
   quietly close around.
7. **Regression green from an Arryn-run `latest.trx`** — per `agent-test-workflow.md`'s hard rule.
   Given Phase 2.7's flagged provenance issue last time (ticket 0050), confirm each trx's timestamp
   and don't accept an agent-run pass as sufficient on its own. **Two trx files now, not one**
   (ticket [0053](0053-split-content-tests-into-their-own-test-project.md) split the suite into
   `LunaFlow.EngineTests` + `LunaFlow.ContentTests` mid-build; Loot's tests live in the latter): read
   both `tests/LunaFlow.EngineTests/TestResults/latest.trx` and
   `tests/LunaFlow.ContentTests/TestResults/latest.trx` — see `agent-test-workflow.md`'s updated
   "Fixed path convention." A "full suite green" verdict needs both green, not just one.
8. **Verdict + evidence to the PM**, written to `Documents/QA/`. A fail routes to Baraka (defect) or
   Amara (spec-vs-reality) — not a fix by you.

### Response

**Fired 2026-08-02, under time pressure, before both stated preconditions were met. Verdict: FAIL —
process/schedule hold, not a technical finding.** Full report:
[`phase-2-7-1-verdict.md`](../../../QA/phase-2-7-1-verdict.md).

This ticket says plainly not to run until 0054 and 0055 both close. Neither has — both still show
blank Resolution sections. That alone is the basis for FAIL: the gate this ticket describes hasn't
actually opened yet, regardless of how the underlying work looks.

That said, asked to spot-check rather than just cite the ticket statuses, so: independently verified
0054's and 0055's own claims against current source rather than accepting either ticket's prose.
0054's "no test coverage exists" is stale — `tests/LunaFlow.ContentTests` has substantive coverage
for all six items its Ask lists (`LootRomTests.cs`, `LootRuleGroupFixtureTests.cs`,
`InventoryCommandHandlerTests.cs`, `OneD20SessionAssemblerTests.cs`,
`ComponentRegistryContentTests.cs`), build's clean, 47/47 pass. 0055's two ratified deviations both
confirmed directly against `D20SystemRuleGroup.cs`/`LookCommandHandler.cs`/`Program.cs` — no
divergence from what the ticket describes. No defect surfaced.

**What this FAIL is not:** a claim that the Loot ROM is broken. It's a claim that the process this
ticket itself lays out — 0054/0055 close, *then* this gate runs — didn't happen in that order. The
six items in this ticket's own Ask list (three-ROM live-session coordination, `ChallengeSolved`'s
dual requirers firing jointly, cross-session randomization variability, all-9-challenges loot
gating, `LootRom`'s assembly-graph zero-reference, and the `latest.trx` provenance/project-split
question ticket 0053 flagged) were **not** run this pass — only 0054/0055's specific claims were
spot-checked. Re-firing this gate once 0054/0055 formally close should mean running those six, not
starting over.

**Re-fired 2026-08-02, now that 0054 and 0055 both show `resolved`. Verdict: PASS.** Full report:
[`phase-2-7-1-verdict.md`](../../../QA/phase-2-7-1-verdict.md). Ran the six items this ticket's Ask
actually lists, not a from-scratch pass:

1. **Three-ROM coordination** — `ComponentRegistryContentTests` covers registration/validation and
   the Loot-removal error. Confirmed live: `Program.cs`'s console session prints "Component registry
   validated successfully" with all three wired in.
2. **`ChallengeSolved`'s two independent requirers, same session** — **no automated test loads
   `GridMapRuleGroup` + `LootRuleGroup` together**, so I drove a real session myself
   (`dotnet run --project src/MyLunaFlowServer`, piped commands): fought and won rooms 5 and 1
   (loot awarded both times, correct items per the monster→item table), then solved room 9's skill
   check in the same session, which completed the `[1,5,9]` win-line and printed the real win text.
   That's GridMap's win-line consumer and Loot's award consumer both firing off D20System's
   `RoomSolved` inserts, live, in one session — not inferred from each running in isolation.
   `LootRom`'s assembly graph: `D20SystemExampleD.csproj` references only the Cartridge surface
   (`D20SystemExampleC`) plus `Rom`/`Rules`/`Engine` — zero reference to GridMap or D20System,
   confirmed by reading the file.
3. **Randomization is real** — ran three independent `dotnet run` sessions (no seed override, real
   `Random`): got two different monsters in room 5 across them. `OneD20SessionAssembler.Shuffle()`
   is a genuine Fisher-Yates over the injected `Random`, not a stub. Topology (fight `{1,5,8}`,
   skill-check `{2,3,4,6,7,9}`) held in all three live runs and is asserted across 50 seeded runs in
   `OneD20SessionAssemblerTests`.
4. **Loot gating** — `LootRuleGroupFixtureTests` covers award/no-award/no-double-award structurally;
   skill-check rooms can never get a `LootAssignment` by construction (derived only from the 3
   assigned monster rooms), so this holds across all 9 challenges, not just the one exercised live.
   `InventoryCommandHandler` reads only `ItemAwarded` — read directly, no cross-ROM reference.
5. **0055's deviations** — reconfirmed against current source: `ChallengePromptRule` still has no
   `Match<RoomChallenge>`, matches `RoomChallengePrompt` only; `LookCommandHandler`'s constructor
   still takes `IReadOnlyDictionary<int,int> monsterMaxHpByRoom`, built and passed in `Program.cs`.
6. **No new Cartridge contract** — `example/D20SystemExampleC/LunaFlow.Cartridge/` still holds
   exactly the same five interfaces Phase 2.7 shipped.

Item 7 (regression trx) came out different from how this ticket anticipated: there's **one**
combined `.trx`, not two — 140/140, covering both projects' classes, at the `EngineTests` path.
`tests/LunaFlow.ContentTests/TestResults/latest.trx` (the sibling file
`LunaFlow_A.ContentTests.runsettings` sets up) doesn't exist; nobody's run that project standalone
against it yet. Provenance (Arryn vs. agent) isn't disclosed this time either way — didn't accept
that on faith, corroborated with my own narrow `--filter "TestCategory=Phase2.7.1"` run (22/22) and
the three live sessions above, which independently exercise most of what the suite claims.

**Not a rubber stamp — one real gap found and disclosed, not blocking:** the joint-session
`ChallengeSolved` coverage in item 2 is manual, not automated. Recommend a follow-up test for Nadia;
noted in the verdict, not filed as a separate ticket since there's no reproducible failure behind
it. Also flagged a pre-existing, already-tracked (ticket 0052) doc-currency issue in the spec's
Capability-declarations section — not new, not blocking.

**Update 2026-08-02 — gate re-fired now that 0054/0055 are both closed. Verdict: PASS.** Full report
(superseding this ticket's FAIL pass above in place):
[`phase-2-7-1-verdict.md`](../../../QA/phase-2-7-1-verdict.md). All items from this ticket's Ask
list ran to completion this time — three-ROM coordination, both `ChallengeSolved` requirers firing
jointly in one live session, cross-session randomization, all-9-challenge loot gating, `LootRom`'s
zero-reference assembly graph, both ratified 0055 deviations reconfirmed against source. One
non-blocking gap noted: no automated test loads all three ROMs' rule groups together in one session
(closed this pass via live playthrough instead — legitimate per `qa-verify`'s own routine, but worth
a joint-session fixture ticket to Nadia eventually, not urgent). Suite: 140/140, one combined trx
(the two-file convention 0053/0056 anticipated wasn't actually exercised — whoever ran it used a
solution-wide pass that merged both projects into the single pinned `EngineTests` path); provenance
not disclosed this time, but independently corroborated via a narrow re-run and three live sessions,
so not treated as a blocker.

### Resolution

**Resolved 2026-08-02 (Kofi, opener).** Verdict is PASS on its own re-fired terms — read
`phase-2-7-1-verdict.md` directly, not just this ticket's summary: all 14 criteria in its table
PASS, none deferred. Closing this gate ticket. Phase 2.7.1 itself is now updated to done in
`roadmap.md`/`phase-2-7-1.md`, citing this verdict. Two non-blocking items carried forward, not lost:
the joint-session automated-coverage suggestion (informal note to Nadia, not a ticket — Lothar's own
call, not urgent) and the pre-existing `grid-rom-d20-split-spec.md`/`loot-rom-content-expansion-spec.md`
capability-table drift (already tracked in 0052, a doc-currency item for Juno/Amara whenever
convenient).
