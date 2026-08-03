# 0030 — Design-doc reconcile after Phase 2.6.5 (running log)

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-26
**For:** Amara (Architect)

## Decision

Running log of doc-accuracy debt across the pages the Architect owns, opened after verifying what
Phase 2.6.5 actually shipped against source. Carries one real ruling: **`IGameSystem` is deleted and
`architecture.md`'s vocabulary stands** — the ROM is the game system. It contradicted that in
compiled public API, and it could never resolve its own "type not yet locked" comment where it sat,
because `Books` wants `IEnumerable<IRuleBook>` and typing it so would require a layer-inverting
`Rules → Rulebook` reference; the deletion went to Baraka via
[0031](0031-code-currency-vs-2-6-5-design.md). Everything else here is reconcile work — one
obligation 2.6.5 assigned the Architect and never landed, `glossary.md` stale in five places, and
the `phase-2-6-5.md`/`phase-2-7.md` corrections — kept on one ticket because it's one concern
(the docs describe the pre-2.6.5 shape); any item that grows into a design question splits out.

## Context

Self-assigned working log for doc-accuracy debt in the pages I own
(`Documents/design/`, `Documents/Specs/`, `Roles/architect.md`). Opened while scoping Phase 2.7;
verifying 2.6.5's as-built shape against `Documents/ApiReference/` and source turned up a batch of
drift, and one design obligation 2.6.5 assigned me that never landed.

Kept as one ticket rather than eight, deliberately: these are all the same concern — "the design docs
describe the shape before 2.6.5 shipped" — and splitting them would produce eight tickets nobody
reads. Per the process's one-concern rule, if any single item below grows into a real design
question, it splits out and gets its own number. **Items get struck through as they land**, so this
doubles as the record of what changed and when.

This ticket is doc work only. Code changes are [0031](0031-code-currency-vs-2-6-5-design.md)
(Baraka); status/roadmap items are [0029](0029-phase-2-6-5-status-and-qa-gap.md) (Kofi).

## Ask

### A. Outstanding obligation from 2.6.5 — mine, never delivered

1. **`engine-core.md` is missing the authored-content-vs-infrastructure clarification.**
   [`phase-2-6-5.md`](../../../Roadmap/phase-2-6-5.md)'s Deliverables state it plainly: a Rulebook
   implementing `IHasDependencies` is *not* a violation of 2.6's "a Rulebook never touches
   `LunaFlow.Engine` primitives directly" rule, because that rule governs *authored content*, not the
   tier's own infrastructure type participating in the engine's module system — and it says this
   should be "recorded next to the original rule in `engine-core.md` so a future session doesn't
   over-read it as a blanket ban." Grepped: it isn't there. The language landed in `IRuleBook`'s XML
   doc remarks instead, which is not where a future session will look. **Record it in
   `engine-core.md`.**

### B. `glossary.md` — stale in five places

2. **`EngineTier`** — described as "the future enum." It's real, in
   `LunaFlow.Engine.Components`, and load-bearing.
3. **`AEngineComponent` / `ARom`** — described as the future base classes. 2.6.5 **rejected** a
   shared base class in favour of each tier project owning its own identity type. Reframe as the
   design's rejected first draft, don't just re-date them.
4. **`Rulebook`** — says `IRuleBook` lives in `LunaFlow.Rules` and is "an empty marker interface."
   It relocated to `LunaFlow.Rulebook` and carries `DependsOnRoms` plus `IHasDependencies`.
5. **`ROM`** — says "not yet a separate project — its concerns currently live as empty placeholders
   (`GameState`, `GameWorld`) directly in `LunaFlow.Engine.Game`." Flatly wrong: `LunaFlow.Rom` is a
   real project with `IRom`, `IInteropRom`, and D20System/NineRooms content.
6. **`Mechanic`** — "currently an empty marker interface" omits ticket
   [0015](0015-imechanic-dice-resolution.md)'s ruling, which is the whole point of its shape
   (`IMechanic : IHasLunaFlowReference`, operations on the concrete class).
7. **Missing entries** — `Capability`, `IHasDependencies`, `ComponentRegistry`, `IRom`,
   `IInteropRom`. All are public surface with no glossary presence.

### C. Reconcile docs to shipped code

8. **`phase-2-6-5.md` records tier-ordering enforcement as deferred; it shipped.**
   `ComponentRegistry.Validate()`'s third pass rejects upward-tier dependencies. Ratify it in the doc
   as landed-early rather than leaving "Explicitly not this phase" contradicting the code. (Not
   asking Baraka to remove it — see [0031](0031-code-currency-vs-2-6-5-design.md).)
9. **`architecture.md`'s external-Rulebook consumption line** — 2.6.5 flagged it as needing
   `LunaFlow.Rulebook` added. **Verified 2026-07-26: already correct** (line 133, with the correction
   note at 135). No action; recorded so it isn't re-checked.

### D. Phase 2.7 doc edits, once the spec exists

10. **`phase-2-7.md` Deliverables** — strike the "shared, ROM-agnostic marker vocabulary in
    `LunaFlow.Rules`" deliverable. Superseded by 2.6.5's `Capability`/`IInteropRom` mechanism, per
    Arryn's ruling (2026-07-26) that **2.6.5 is the authority** where the two conflict. 2.6.5
    explicitly rejected content-shaped abstractions in `LunaFlow.Rules` — "that project is NRules
    infrastructure, not a content home, however abstract the content gets." Strike-through with a
    pointer, per house convention; don't delete.
11. **`phase-2-7.md` Done-When rewording** — 2.6.5 flagged this as an open follow-up: "neither ROM
    references the other's assembly" needs restating to the real rule (no mutual or direct peer
    reference; one-directional client→`IInteropRom` references are expected and governed).
    Mechanism language is mine; the **scope** strikes (Nine Rooms retrofit removal) are Kofi's row
    in [0029](0029-phase-2-6-5-status-and-qa-gap.md), not mine to write unilaterally.

### E. Open design question — **RULED 2026-07-26, did not need to split**

12. **`IGameSystem` needs a ruling.** Public interface in `LunaFlow.Rules`, absent from every design
    doc and from the glossary, marked "placeholder for future work (Phase 3+), not currently in
    use." Four problems, in descending order of how much they bother me:

    - **It contradicts `architecture.md` on the meaning of "game system."** The design doc is
      explicit that **the ROM is the game system** — "the difference between playing D&D and playing
      Monopoly" — with Rulebook as the setting built on top. `IGameSystem` defines it as "a named
      system made up of one or more rulebooks," a container *above* Rulebook. Two contradictory
      definitions of one term, one in prose and one in compiled public API. This is exactly the
      ROM/Rulebook blurring `architecture.md`'s "System, setting, and session" section says it exists
      to prevent.
    - **It's in `LunaFlow.Rules`** — a content-organization type inside the boundary 2.6.5 just
      closed against exactly that (see item 10).
    - **`IEnumerable<object> Books`** defers its own type decision with the comment "type not yet
      locked; see Phase 2.6.5." 2.6.5 is closed and never touched it.
    - **It's `public` on the authoring surface** without the deliberate allowlist decision ticket
      [0014](0014-engine-api-surface-scope.md) established as the standard for that.

    Resolve the term collision first — that's the load-bearing part, and 2.7 is where tier identity
    gets settled for real, so it wants an answer before the spec lands. Any resulting code change
    (relocate, retype, or delete) goes to Baraka; [0031](0031-code-currency-vs-2-6-5-design.md)
    carries it as a flagged inbound, not yet an ask.

    ---

    **RULING (Amara, 2026-07-26; Arryn concurring — "vestigial, an unimplemented concept that got
    smuggled in somehow").** Ruled in place rather than split out: one decisive structural fact
    collapsed it from a design question into a cleanup.

    **The decisive fact: `IGameSystem` cannot resolve its own deferred decision where it sits.**
    `Books`'s comment reads "type not yet locked; see Phase 2.6.5." The honest type is
    `IEnumerable<IRuleBook>` — but `IRuleBook` lives in `LunaFlow.Rulebook`, and `LunaFlow.Rules`
    sits *below* that tier (Rulebook reaches Rules transitively via Rom; `LunaFlow.Rulebook.csproj`
    references Engine and Rom, not Rules directly). Typing `Books` correctly would require
    `Rules → Rulebook`, inverting the layer order. That is why it is `IEnumerable<object>`, and why
    2.6.5 could not lock it — not an oversight, a structural impossibility. A placeholder that can
    never be filled in place is not a placeholder.

    1. **`architecture.md`'s vocabulary stands.** The **ROM is the game system** (D&D vs. Monopoly);
       the Rulebook is the setting built on top. `IGameSystem` uses "system" in the pre-settled
       sense, from before "System, setting, and session" firmed up — consistent with it sitting
       among the day-one interfaces (`ILunaFlowReference`, `IMechanic`, `IRuleGroup`). No change to
       `architecture.md`; the prose was right and the code was stale.
    2. **Delete `IGameSystem`.** Grepped the whole repo: exactly one reference, its own declaration
       — no consumer, no test, no design-doc mention. Unused public surface that contradicts the
       settled vocabulary and cannot be typed correctly where it lives is not worth carrying, and
       [0014](0014-engine-api-surface-scope.md) already established that public authoring surface is
       a deliberate decision rather than an accretion. Code change handed to Baraka as a real ask —
       [0031](0031-code-currency-vs-2-6-5-design.md), item 4.
    3. **If the concept is ever wanted**, "a named collection of rulebooks" is the packaging/manifest
       concern [`non-goals.md`](../../../design/non-goals.md) already defers with a named trigger —
       "not building config-driven ROM/Rulebook packaging yet," revisited when a second
       Rulebook/ROM combination needs shipping. It would land **above** the Rulebook tier, not in
       `LunaFlow.Rules`. Intent preserved; speculative surface not.

    **Follow-on doc work this ruling creates (mine, tracked under item 7 above):** add a
    **`Game system`** entry to `glossary.md` pinning the term to the ROM, with a note that a
    now-deleted `IGameSystem` once used it the other way and why that direction was rejected. The
    glossary is where a future session will look before it re-invents this; a ticket is a ledger
    entry, not the record.

### F. My own role page

13. **`Roles/architect.md`'s bootstrap prompt omits `Documents/README-QUICK-START.md`**, which
    `CLAUDE.md` flags as **"Start here if you're new."** I didn't read it during orientation because
    the prompt didn't list it. Add it to the Orient block. (The PM's prompt has the same omission —
    Kofi's page, Kofi's fix; not raised in 0029 as it's cosmetic.)

## Response

**2026-07-26 — items 1–13 landed.** Full pass; nothing from the original Ask outstanding.

- **1 (`engine-core.md`)** — new section "Authored content vs. infrastructure — what the
  Rulebook/Engine rule actually bans," placed immediately before "ROM as the conduit to NRules" so it
  sits next to the rule it qualifies, as 2.6.5 asked. Names the two adjacent non-violations
  (transitive `Engine.dll` via ROM; composition-root exemption) and gives a test to apply — *is this
  the content an author writes, or the plumbing that lets the tier exist?*
- **2–7 (`glossary.md`)** — `EngineTier`, `Mechanic`, `ROM`, `Rulebook` rewritten to shipped reality;
  `AEngineComponent`/`ARom` reframed as ⚠️ **rejected design, kept for lineage** rather than re-dated,
  since 2.6.5 didn't defer them, it rejected the approach. Five new entries added: `Capability`,
  `ComponentRegistry`, `IHasDependencies`, `IInteropRom`, `IRom`. `IInteropRom` carries an explicit
  ⚠️ that the broker behaviour doesn't exist and the name shouldn't be read as callable.
- **8 (`phase-2-6-5.md`)** — upward-tier deferral struck and **ratified as shipped-early**, with the
  original reasoning left struck-through rather than deleted. Recorded *why* it's worth writing down:
  doc and code disagreed in two directions at once — this pass shipped though deferred, the specified
  cycle check didn't ship at all.
- **9 (`architecture.md`)** — verified correct, no action, as anticipated.
- **10–11 (`phase-2-7.md`)** — done now rather than waiting for the spec, since Kofi had already
  restructured the surrounding text in [0029](0029-phase-2-6-5-status-and-qa-gap.md) and leaving the
  marker bullet unstruck beside his new broker bullet would have read as two live, competing
  mechanisms. Marker-vocabulary deliverable struck with the reasoning preserved — **the pattern
  survives, the location doesn't** — and the Done-When's "and/or fact/marker-mediated" residue
  removed with it. Reference rule now stated precisely (peer-to-peer edge is the bar; one-directional
  ROM→broker is expected and governed). The broker's own acceptance clause is deliberately left
  open and flagged inline, since it can't be written before the surface is specced.
  - **Found while editing:** the `IRuleBook` deliverable is **half-shipped** — `DependsOnRoms` is
    declared but nothing validates it, so a Rulebook naming an unregistered ROM fails silently, which
    is the exact "silent degradation" that bullet rules out. Noted in place; still live 2.7 work.
- **12 (`IGameSystem`)** — ruled above; `Game system` glossary entry added pinning the term to the
  ROM and recording why the container-of-rulebooks reading was rejected, so the ruling lives in a
  design doc rather than only in this ticket.
- **13 (`architect.md`)** — `README-QUICK-START.md` added to the bootstrap's Orient block, with a
  note that it's easy to skip past.

**Verified with `doc-lint`** (`Documents/ClaudeWiki/Tools/doc-lint.py`) rather than by eye. That
surfaced a pre-existing problem worth a separate raise — see the new item below.

### G. New — surfaced by `doc-lint`, mostly not mine (2026-07-26)

14. **~110 broken relative links across `Documents/`, in three systemic patterns**, none introduced
    by this pass but two of which I propagated by copying an existing convention without checking it:

    - **`../../Roadmap/...` from `Handoffs/`** resolves to `ClaudeWiki/Roadmap/`, which doesn't
      exist; it needs `../../../`. Affects 0023, 0024, 0025, 0028 — and affected 0029/0030/0031,
      which I copied the pattern from 0025 into. **My three are fixed.** The others are closed
      tickets belonging to other roles; left alone.
    - **`../Specs/nine-rooms-spec.md`** — that spec actually lives in `Documents/NineRooms/`. Fixed
      in the two files I own (`engine-core.md`, `glossary.md`); still broken elsewhere.
    - **Roadmap phase docs linking `design/...` and `Specs/...` without `../`** — Kofi's pages.

    Also two orphans, one of which is mine: **`Documents/Specs/engine-host-spec.md` is linked from
    nothing** — because `phase-1-1.md`'s link to it is one of the broken ones above. The spec the
    Architect role page holds up as its precision exemplar is currently unreachable by navigation.

    **Raise to Kofi**, not fixed here: the bulk sits in Roadmap and wiki pages he keeps, and a
    sweeping cross-cutting link fix by the Architect is exactly the kind of silent tidying that makes
    a ledger untrustworthy. Recommend it goes out as its own ticket with the `doc-lint` output
    attached. *(Note for whoever picks it up: `doc-lint`'s other 27 findings — tickets missing a
    `## Decision` section — are the known pre-Convention-4 backlog, not fresh defects. Don't chase
    them as part of this.)*

## Resolution

Closed 2026-07-30. Re-checked rather than taken on the Response's word:

- Items 1–13: spot-checked `engine-core.md`'s "Authored content vs. infrastructure" section,
  `glossary.md`'s `Capability`/`ComponentRegistry`/`IHasDependencies`/`IInteropRom`/`IRom`/
  `Game system` entries, and `phase-2-6-5.md`/`phase-2-7.md`'s struck-and-reasoned edits — all
  present as described. `IGameSystem`'s deletion is Baraka's territory (ticket 0031, resolved) and
  not re-verified here; the doc-side ruling it's built on stands.
- Item 14 (broken links): confirmed routed to and closed via
  [ticket 0035](0035-broken-relative-links-across-documents-cleanup-pass.md), resolved by Kofi.
  Nothing left unrouted.

No open items remain against this ticket's original Ask. Closing as the long-running doc-currency
pass it was scoped to be — new drift from here forward gets its own ticket rather than reopening
this one, per the one-concern rule this ticket already held itself to.
