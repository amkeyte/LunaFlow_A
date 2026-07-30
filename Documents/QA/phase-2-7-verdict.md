# Phase 2.7 (Multi-ROM composition) — QA Verdict

**Verdict:** PASS — process flag below is now closed, not just non-blocking.
**Date:** 2026-07-29 (original pass, FAIL); updated 2026-07-29 (2 of 4 blockers cleared, still FAIL); **final update 2026-07-30 (PASS)**; **process flag closed 2026-07-30 (Arryn-run full pass, 119/119)**
**Ticket:** Answers the standing gate [0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md).
**Build:** `dotnet build LunaFlow_A.sln` — clean, 0 errors (re-run this session).
**Suite:** Read from `tests/LunaFlow.EngineTests/TestResults/latest.trx`, timestamped 2026-07-30 00:58:22. **119/119 passed, 0 failed.**

## Process flag — closed 2026-07-30

Arryn ran a full clean pass herself: **119/119 passed.** That's the genuinely user-run `latest.trx`
the note below was asking for — the flag is closed, not merely non-blocking. Left the original note
below intact rather than deleted, per the supersede-don't-delete convention, since it explains why
the flag existed in the first place.

Per ticket [0050](../ClaudeWiki/Processes/Handoffs/0050-test-harness-needs-roomchallengeprompt-gameendannouncement-seeds.md)'s
own Resolution text, the full-suite run that produced this `latest.trx` was run solo by the closing
agent ("ran the whole thing here"), not by Arryn via VS2022. That is a violation of
`agent-test-workflow.md`'s hard rule — a "full suite green" doneness claim is supposed to be
user-run, precisely so QA isn't trusting an agent's self-report of its own work. I'm not treating it
as invisible: flagging it plainly, and not accepting it as evidence on its own.

What actually grounds this verdict is independent re-verification, not the trx's provenance:
- Ran `dotnet build LunaFlow_A.sln` myself this session — clean.
- Ran `dotnet test --filter "FullyQualifiedName~ComponentRegistryTests"` myself — 23/23.
- Ran `dotnet test --filter "TestCategory=Rom_OneD20"` myself — 25/25, matching ticket 0045/0050's claims exactly.
- Reproduced both 0046/0047's fault-injection claims by hand in the previous pass (missing-ROM error, deleted-assembly binding failure) — not repeated this pass since nothing in 0043/0049/0050 touched that code again.
- Read every changed source file directly (`Facts.cs`, `D20SystemRuleGroup.cs`, `GridMapRuleGroup.cs`, `Program.cs`, both test harness files) rather than trusting ticket prose.

The trx's raw numbers are corroborated by narrower runs I ran myself; the only thing not independently
re-established is the *entire* 119-test run in one sitting, which the hard rule reserves for Arryn.
**Recommend Kofi have Arryn run one full pass from VS2022 to produce a genuinely user-run `latest.trx`
before this phase is held up as a model of house process** — but I'm not blocking the technical
done-bar on it, since I've substantively verified the content it claims through my own means.

## Criteria

| Criterion | Evidence | Result |
| --- | --- | --- |
| One Rulebook depends on and coordinates two independently-loadable ROMs | `OneD20RuleBook.DependsOnRoms = ["GridMap","D20System"]`; `Program.cs` constructs, registers, and drives both through all five commands | PASS |
| Neither ROM references the other's assembly | `D20SystemExampleA.csproj`/`D20SystemExampleB.csproj` — no A→B or B→A reference, both reference `D20SystemExampleC` only | PASS |
| Interaction is capability-mediated through Provides/Requires + contract types | `GridMapRom`/`D20SystemRom.Provides`/`Requires` declared directly against each other; `ComponentRegistry.Validate()` validates the pair | PASS |
| Removing either ROM produces a clear, immediate error (`DependsOnRoms` actually validated) | `ComponentRegistry.ValidateDependsOn(...)` (ticket 0047), called in `Bootstrap()` before `Validate()`. Independently reproduced last pass: commented out GridMap's registration, got the exact attributable error, reverted | PASS |
| `LunaFlow.Rulebook` names no ROM assembly | csproj references only `Engine`/`Rules`; grepped for `using LunaFlow.Rom`/`LunaFlow.Cartridge` — zero hits | PASS |
| Two peer ROMs correlate via contract type, zero project reference | Same csproj evidence; `PlayerPosition : IPlayerLocation`, `RoomSolved : IChallengeSolved`, `CombatRound : ICombatStatus`, `WinMarker : IGameWon` all confirmed in `Facts.cs` | PASS |
| Both `Match<T>` (rule) and `Query<T>` (handler) cross-ROM paths exercised | `Match<IPlayerLocation>`/`Match<IChallengeSolved>` in rule groups; `Query<IPlayerLocation>`/`Query<ICombatStatus>` in handlers | PASS |
| No lookup API exists anywhere in the shipped surface | Grepped `src/`/`example/` for `Resolve(`/`Resolve<`/`IInteropRom` usage — zero hits outside the now-empty marker interface itself | PASS |
| Consuming ROM obtains peer's service impl only via constructor argument from `Program.cs` | `FleeCommandHandler(IPlayerMovement, ...)`; `Program.cs` constructs `GridMovementService` directly | PASS |
| Contract surface implements no `IRom`, not registered | `example/D20SystemExampleC/LunaFlow.Cartridge/` — five bare interfaces, no `IRom`; `ComponentRegistry` registers only GridMap/D20System/OneD20RuleBook | PASS |
| Rulebook references no ROM implementation assembly | Same csproj/grep evidence as above | PASS |
| `D20SystemContent.Configure` gone from both content ROMs | Grepped `example/` — zero hits; class retained, obsolete, in `src/LunaFlow.Rom` for NineRooms regression only (ticketed exception) | PASS |
| Bootstrap probe (§5) runs and fails loudly against a deliberately broken build | `Program.cs`'s `Bootstrap()`/`RunGame()` split (ticket 0046, against 0048's `Capability.Provided<,>`/`ForceMemberResolution()` ruling). Independently reproduced last pass: deleted the built Cartridge assembly, ran the exe directly — binding failure named module + exception, real exit code 1, confirmed unmasked | PASS |
| Probe forces member resolution via `GetInterfaceMap`+`PrepareMethod` | `Capability.ForceMemberResolution()`, called per `Provides` entry inside `Bootstrap()`'s try block | PASS |
| Full suite green from an Arryn-run `latest.trx` | 119/119, fresh — Arryn ran a clean full pass herself 2026-07-30, closing the provenance flag raised over ticket 0050's agent-run trx | PASS |
| `IRom` instances exist for GridMap/D20System only; Cartridge implements no `IRom`, not registered | Confirmed via `GridMapRom : IRom`, `D20SystemRom : IRom`, 3 total registrations | PASS |
| Every behavior in `1-d20-spec.md`'s acceptance criteria holds unchanged | **RESOLVED via ticket [0043](../ClaudeWiki/Processes/Handoffs/0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md).** Challenge-prompt rules and `GameEndRule` now match `RoomChallengePrompt`/`GameEndAnnouncement` facts carrying real, already-resolved flavor text (`Facts.cs`, `D20SystemRuleGroup.cs`, `GridMapRuleGroup.cs` all read directly, confirmed). `Program.cs` seeds them from `flavorText["monster-desc"]`/`flavorText[r.PromptKey!]`/`flavorText["win-announce"]` — all three keys confirmed present in `1-d20-flavor.md` | PASS |
| Challenge-prompt/win-line rules fire off contract types, zero-reference | Unchanged from prior pass, re-confirmed | PASS |
| `Not<IGameWon>()` guard present, challenge prompts stop once `WinMarker` exists | `D20SystemRuleGroup.cs` — `Not<IGameWon>()` present, `WinMarker : IGameWon` | PASS |
| `look`'s combat-status line via `ICombatStatus`, no GridMap→D20System reference | `LookCommandHandler.cs` — `Query<ICombatStatus>()` | PASS |
| `flee` returns to `PreviousRoomId` via `IPlayerMovement`, false-case no-op | `GridMovementService.cs` — exact shape specced | PASS |
| `LunaFlow.Cartridge`/`D20SystemExampleC` has zero project reference to GridMap or D20System | `D20SystemExampleC.csproj` — zero `ProjectReference` entries | PASS |
| No `LunaFlow.Rulebook` source touches `ICommandHandler`/`LunaFlowEngine.Host`/`RuleEngine` directly | Grepped — zero hits | PASS |
| Two-phase validator fails fast on missing capability, duplicate provider, cycles | `ComponentRegistry.Validate()` — all four checks present, `ComponentRegistryTests` (23/23, re-run this session) covers it | PASS |
| No vestigial/orphan project contradicting a documented ruling | **RESOLVED via ticket [0049](../ClaudeWiki/Processes/Handoffs/0049-delete-orphan-src-lunaflow-cartridge-project.md).** `src/LunaFlow.Cartridge` and its `.sln` entry (including the `GlobalSection(ProjectConfigurationPlatforms)` lines the first resolution attempt missed) are fully gone — confirmed by grepping `LunaFlow_A.sln` for `Cartridge`: zero hits. `dotnet build` clean | PASS |

## Deviations from spec (informational, not blocking)

- `D20SystemContent` is retained (obsolete, object-typed) in `src/LunaFlow.Rom` solely for NineRooms regression support — a deliberate, ticketed exception (0027, CodePlan 0037).
- Phase 2.6.5 (the mechanism 2.7's Done-When is explicitly built on) has no standalone QA verdict in `Documents/QA/index.md`. Not litigated as part of this verdict — flagged so it isn't lost.
- The generated-rule flavor-text seam (RoomChallengePrompt/GameEndAnnouncement) is a per-case pattern, not a general one — deliberately, per ticket 0043's own "addendum, scaling" note. Revisit if a third case appears.

## Superseded reports

None — this file has been updated in place across the three passes rather than archived, since all three shared one live investigation. Prior verdict text (both FAIL states) is preserved in git history, not deleted.

## Tickets filed / closed

- [0046](../ClaudeWiki/Processes/Handoffs/0046-bootstrap-contract-probe-not-built.md) — resolved, closed by QA after independent fault-injection reproduction.
- [0047](../ClaudeWiki/Processes/Handoffs/0047-dependsonroms-validation-not-enforced.md) — resolved, closed by QA after independent fault-injection reproduction.
- [0043](../ClaudeWiki/Processes/Handoffs/0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md), [0048](../ClaudeWiki/Processes/Handoffs/0048-capability-provider-model-vs-bootstrap-probe-spec.md), [0049](../ClaudeWiki/Processes/Handoffs/0049-delete-orphan-src-lunaflow-cartridge-project.md), [0050](../ClaudeWiki/Processes/Handoffs/0050-test-harness-needs-roomchallengeprompt-gameendannouncement-seeds.md) — all resolved by their own opener/assignee chain; content independently re-verified here, not taken on their word.
- **No new ticket filed this pass.** The one open item — 0050's suite run being agent-run rather than Arryn-run — is a process-hygiene note for Kofi, not a defect with an owner to fix in code.

This verdict fills `Documents/QA/index.md` as the current entry for Phase 2.7, and fills the `## Response` section of ticket [0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md) — Kofi closes 0025, QA doesn't close its own gate.
