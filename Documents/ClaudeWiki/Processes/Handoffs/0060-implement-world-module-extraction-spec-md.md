# 0060 — Implement world-module-extraction-spec.md

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-08-02
**For:** Baraka (Impl Dev)

## Decision

Built in full, including item 5: Hollow Keep's content landed mid-build (0059 delivered, 0061 fixed
its fixed-vocabulary key bug) so both `IOneD20WorldModule` implementations shipped in this pass
instead of only `DefaultOneD20WorldModule`. Both playtested end-to-end; the validator (with the
2026-08-03 fixed-vocabulary check folded in) demonstrated rejecting two distinct broken-module
scenarios through the real `Program.cs` pipeline, not just in isolation. One expected, out-of-scope
casualty: `tests/LunaFlow.ContentTests` no longer compiles (the statics it referenced are gone),
flagged to Nadia as [0062](0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md) —
this ticket's own last acceptance bullet ("full suite green") waits on that landing.

## Details

### Context

Phase 2.8 ([`phase-2-8.md`](../../../Roadmap/phase-2-8.md)) is design-complete and ready to build —
Amara's design (`design/world-module.md`) and buildable spec
([`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md)) are both in
place, reviewed, and unblocked now that Phase 2.7.1 has closed (see
[0057](0057-phase-2-8-design-package-ready-for-kickoff.md)). No new framework capability anticipated
— this is Rulebook-tier restructuring only, same "toolbox-first" discipline the last two content
phases held.

### Ask

Build [`world-module-extraction-spec.md`](../../../Specs/world-module-extraction-spec.md) in full
against its own acceptance criteria:

1. **`IOneD20WorldModule`** contract in `LunaFlow.Rulebook.OneD20`; **`DefaultOneD20WorldModule`**
   reproduces today's exact shipped content (9 rooms, 9 challenges, 5 monsters, loot table, PC
   stats, flavor text) — verified by diff against `OneD20RulebookContent`/`OneD20FlavorText`'s
   current values, not just a shape match.
2. **Retire the static surface** — `OneD20RulebookContent`/`OneD20FlavorText` gone; zero references
   left in `LunaFlow.Rulebook`, `example/`, or `src/MyLunaFlowServer`.
3. **Decouple `OneD20SessionAssembler`** — takes an `IOneD20WorldModule`, no references to the old
   statics by name.
4. **A load-time validation pass** at the World Module boundary (spec's Mechanics section lists the
   checks) — demonstrate it rejects a deliberately broken module with a clear, non-zero-exit error.
5. **A second, independently-authored `IOneD20WorldModule`** proving the swap — content is Juno's
   call, tracked separately as [0059](0059-second-world-module-reskin-content.md); don't block
   starting 1–4 on it landing, but the phase's actual Done-When bar (full session, zero Rulebook/ROM
   changes) needs it before this ticket can close.

If content/implementation surfaces a real framework gap the spec's toolbox can't express, that's a
ticket to Amara, not a workaround — same discipline `loot-rom-content-expansion-spec.md` held.

Once both `IOneD20WorldModule` implementations run clean and the validation pass is demonstrated:
full suite green, read from an Arryn-run `latest.trx`, per the spec's own last acceptance item.

### Response

All five items built. Implementation plan tracked in
[`CodePlans/2-8.md`](../../../Roadmap/CodePlans/2-8.md) (revised twice mid-build: once for the
2026-08-03 fixed-vocabulary validator amendment, once for 0059's content landing).

1. **Contract + `DefaultOneD20WorldModule`.** `IOneD20WorldModule` in `LunaFlow.Rulebook.OneD20`;
   `DefaultOneD20WorldModule` (partial class, `DefaultOneD20WorldModule.cs` +
   `DefaultOneD20WorldModule.FlavorText.cs`, mirroring the old file split). Verified by scripted diff
   against `OneD20RulebookContent`/`OneD20FlavorText`'s content before deleting them — data-bearing
   lines identical, only the static-field → instance-property declaration syntax differs.
2. **Static surface retired.** `OneD20RulebookContent.cs`/`OneD20FlavorText.cs` deleted. Grepped
   `src/` post-deletion: zero code references (`OneD20RuleBook.cs`'s doc comment, the only other hit,
   used a `<see cref>` to the deleted types — fixed to avoid a dangling-cref warning).
3. **`OneD20SessionAssembler` decoupled.** Constructor takes `IOneD20WorldModule worldModule` first,
   `Random?` second; `Assemble()` reads all four pools from the injected module. Grepped the file for
   the old type names post-change: zero hits.
4. **Validation pass.** `OneD20WorldModuleValidator.Validate(IOneD20WorldModule)`, standalone static
   class, six fail-fast checks (room keys, pool-count floor, room-type-vs-pool fit, per-template
   flavor keys — including room `DescriptionKey`, one check beyond the spec's literal list, flagged
   below — ability-score completeness, and the eleven fixed-vocabulary keys from the 2026-08-03
   amendment). Demonstrated against the real `Program.cs` pipeline, not just the validator in
   isolation: a structural gap (`Challenges` trimmed to 5) and a fixed-vocabulary gap (`win-announce`
   renamed) each produced a load-time `InvalidOperationException`, a console message naming what's
   wrong, and a non-zero process exit — confirmed by temporarily wiring a throwaway broken module into
   `Bootstrap()`, running it, and reverting.
5. **Second implementation.** `HollowKeepWorldModule` (same partial-class split), content transcribed
   verbatim from `1-d20-worldmodule-hollow-keep-spec.md`/`-flavor.md` — numeric fields (DCs, monster
   stats, PC sheet, ability scores) diffed against `DefaultOneD20WorldModule`'s and confirmed
   byte-for-byte identical, per that spec's own acceptance criteria. Flavor text checked for leftover
   warehouse-specific nouns (crate, rapier, etc.) — none found.

**Playtesting (both modules):** ran full sessions via piped console input against each — move, look,
check (success/failure/retry), fight (a win, a loss, and a proper mid-combat flee-and-later-return),
flee, inventory/inv all exercised; loot awarded only for monsters actually defeated (not the lost or
fled fights); flavor text resolved with zero missing-key errors in either module; win-announce fired
with each module's own text. Compile-time choice left on `DefaultOneD20WorldModule` (unchanged
player-visible behavior, per the spec's "Supersedes: nothing"), with `HollowKeepWorldModule` as the
documented one-line alternative (`Program.cs`'s `Bootstrap()`, top of the method).

**Judgment call to flag:** the validator's flavor-key check also covers each room's `DescriptionKey`,
which the spec's literal Mechanics bullet list doesn't name (only challenge/monster template keys are
listed there). Kept it — same failure category, same boundary, and `Program.cs` reads
`DescriptionKey` unchecked today — but noting it explicitly since it wasn't specced.

**Build:** `src/`/`example/` clean, 0 warnings/0 errors. `tests/LunaFlow.ContentTests` fails to
compile — expected, out of territory, flagged as
[0062](0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md). This ticket's
last acceptance bullet ("full suite green, Arryn-run `latest.trx`") waits on 0062 landing before I'd
call this fully closed.

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
