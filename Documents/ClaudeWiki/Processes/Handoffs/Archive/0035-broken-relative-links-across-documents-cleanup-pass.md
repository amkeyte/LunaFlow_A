# 0035 — Broken relative links across Documents/ — cleanup pass

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-26
**For:** Kofi (PM)

## Decision

**Fixed the systemic root cause, logged what's left.** `doc-lint.py --check links` found 108 broken
relative links. 24 of them (across 12 Handoffs tickets) shared one root cause — `../../Roadmap/`,
`../../design/`, `../../QA/`, `../../Specs/` from `Handoffs/*.md`, which needs three `../` not two to
reach `Documents/`. Fixed by a targeted script matching only `](../../<folder>/` (so it couldn't
double-mangle an already-correct `../../../` link). Re-ran doc-lint: 108 → 84. Remainder is smaller,
older, and lower-value to chase — itemized below rather than fixed blind.

## Details

### Context

Raised by Amara in [0030](0030-doc-reconcile-post-2-6-5.md) item G, item 14: running `doc-lint.py`
against her own doc-reconcile pass surfaced ~110 broken links repo-wide, only some of which she
introduced (she fixed those in the three files she touched). Her read: "the bulk sits in Roadmap and
wiki pages he keeps, and a sweeping cross-cutting link fix by the Architect is exactly the kind of
silent tidying that makes a ledger untrustworthy." Recommended it go out as its own ticket rather than
get folded into 0030's close. This is that ticket, self-opened rather than routed elsewhere since it's
housekeeping on pages the PM keeps (Roadmap, Handoffs).

This also closes the loop on a finding from earlier this session (documented in
[`doc-lint-SKILL.md`](../../Tools/doc-lint-SKILL.md)'s "Known findings" table): 84 broken links found
then, same root cause identified, not yet actioned. It's actioned now.

### Ask — done, logged for the record

**Fixed:** the `../..` → `../../..` pattern in 12 Handoffs tickets (0007, 0008, 0010, 0011, 0022,
0023, 0024, 0025, 0026, 0028, 0032, 0033 — 24 links), plus one one-off in 0032
(`../Processes/wiki-style.md` → `../wiki-style.md`, an extra path segment rather than a missing one).
Mechanical, path-only — no ticket content changed, per the same reasoning that governs correcting a
closed ticket's substance (this isn't that; this is fixing navigation, not the record).

**Not fixed — logged for whoever picks this up next:**

1. **4 links, same-shaped bug, different old tickets** (0002, 0004, 0011 — all pre-2026, in
   `index-archive.md`'s range): `../../../Documents/QA/...` carries an extra `Documents/` segment;
   correct is `../../../QA/...`. Low value to chase — three closed, archived tickets from Phases
   1.5/2.
2. **~20 links in `Documents/Roadmap/phase-*.md`** pointing to `design/...` or `Specs/...` without a
   leading `../` — these are Kofi's own pages (roadmap phase docs), not Handoffs. Same shape of bug,
   different owner-relative-path miscount.
3. **`Documents/NineRooms/*.md`** (4 links) and **`Documents/Specs/engine-host-spec.md`** (1 link) —
   predate this session, reference `Specs/nine-rooms-spec.md`/`engine-host-spec.md` paths that moved
   when Nine Rooms was deprecated and its spec relocated. Nine Rooms is retained for regression only
   (per [0029](0029-phase-2-6-5-status-and-qa-gap.md)); low priority.
4. **`doc-lint-SKILL.md`'s own flagged "broken" links** (6 of the 84) are intentional — they're the
   documented hypothetical output examples from the tool's own README-equivalent, not real bugs.
   `doc-lint.py` can't distinguish a documented example of its own output from a real link; not worth
   teaching it to.

Items 1–3 are the same mechanical shape as the fix already done — whoever picks this up can likely
reuse the same script pattern. Not chased further this pass because the marginal value (fixing
navigation on old, closed, or deprecated pages) is low relative to the review overhead of getting each
one's correct relative depth right by hand.

### Response

N/A — opened and closed by the same person as a working log, per the pattern
[0030](0030-doc-reconcile-post-2-6-5.md) set.

### Resolution

**Resolved 2026-07-26 (Kofi, opener).** Root-cause fix landed and verified (108 → 84 via a fresh
`doc-lint.py` run). Remainder itemized above rather than silently left; revisit opportunistically
when touching any of the affected pages for other reasons, not as standing work.
