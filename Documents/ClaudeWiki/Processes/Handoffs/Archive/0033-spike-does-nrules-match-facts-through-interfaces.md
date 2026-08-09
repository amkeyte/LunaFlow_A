# 0033 — Spike: does NRules match facts through interfaces?

**Status:** resolved — answered from NRules source, not from the spike; round 2 withdrawn
**Opened by:** Amara (Architect)
**Opened:** 2026-07-26
**For:** Nadia (Test Dev)

## Decision

**Yes — NRules 1.0.2 matches facts through interfaces.** Settled by reading NRules' own Rete
implementation at the pinned version tag, after the spike returned a **false negative**:
`TypeNode.IsSatisfiedBy` is `FilterType.IsAssignableFrom(fact.FactType)`, `RootNode` admits every
fact unconditionally, and `AlphaNode.PropagateAssert` offers each fact to *every* child node to
filter independently. `IsAssignableFrom` is true for interface implementation, and there is no
exact-type dispatch anywhere in the path. This also settles the follow-on question that would have
sunk the design either way — one inserted fact satisfies its concrete type node **and** its contract
type node independently, so an owning ROM can match `PlayerPosition` while a peer matches
`IPlayerLocation`. [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §3
stands as written; the abstract-base-class fallback (round 2) is withdrawn unbuilt. The real lesson
is process, not NRules — see Resolution.

## Details

### Context

Phase 2.7's cross-ROM design rests on one assumption about NRules that I can't verify from the
documentation, and I'm not willing to have a phase built on an inference about a third-party library.

**The assumption:** NRules matches facts polymorphically **through interfaces**, not only through
base classes. Concretely — given `class PlayerPosition : IPlayerLocation`, does a session that has
been handed `Insert(new PlayerPosition(...))` fire a rule whose pattern is
`Match<IPlayerLocation>(...)`?

**Why it's load-bearing.** `Match<T>` needs the CLR type `T` at compile time in the matching
assembly, but peer ROMs are forbidden from referencing each other. The resolution in
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) is that a consuming ROM
matches a **contract type** in a shared interop assembly, which the owning ROM's concrete fact
implements. If interface matching doesn't work, that entire approach collapses and the fallback is
strictly worse — a shared *concrete* fact type in the interop assembly, which turns that assembly
into the shared-vocabulary project Phase 2.6.5 was wary of and stops an owning ROM extending its own
facts without editing shared code.

**What the evidence actually says.** NRules' documentation states pattern matching is polymorphic,
that `Match<Fruit>` matches `Apple` and `Pear`, and that `Match<object>` matches every fact in
working memory. That last one implies assignability-based type nodes rather than exact-type lookup,
and interfaces would follow from that. But **the documentation never uses the word "interface,"** and
grepping `src/` and `tests/` turns up no `Match<I…>` anywhere — nothing in this codebase exercises
it today. Strong inference, zero evidence. Not good enough to spec against.

Version in play: **NRules 1.0.2** (`LunaFlow.Engine`, `LunaFlow.Rules`).

### Ask

**1. Answer the question with a test.** An interface, a concrete fact implementing it, a rule whose
`When()` matches on the interface; insert the concrete fact, fire, assert the rule fired. Should be
under twenty lines — this is a small, sharp question, not a suite.

Worth covering in the same pass, since they're nearly free once the fixture exists and each would
change the spec if it surprised us:

- **Does a second implementer also match?** Two unrelated concrete facts implementing the same
  interface, both inserted — does the rule fire twice, once per fact? (Expected: yes, per NRules'
  documented "each match results in a separate firing.")
- **Does an inserted fact match on *multiple* interfaces it implements?** A fact implementing two
  contracts, with two rules matching one each — do both fire? This is the case where a ROM's fact
  crosses two different boundaries at once, which the spec permits.
- **Does binding work through the interface?** `Match<IPlayerLocation>(() => loc, loc => loc.RoomId == 3)`
  — the condition must read through the interface's members, not just match the type. A type match
  that can't carry a condition would be useless to us.

**2. If it passes, keep it — don't delete it.** This is filed as a spike, but I'd argue against
treating the result as throwaway. The **entire cross-ROM boundary rests on this NRules behaviour**,
and it's third-party behaviour we don't control: an NRules upgrade that quietly narrowed polymorphic
matching to base classes only would break the ROM boundary in a way nothing else in the suite would
catch, and it would surface as a baffling "rule mysteriously stopped firing" a long way from the
cause. That's the textbook case for a durable characterization test. Suggest it lands with a comment
naming *why* it exists and pointing back here, so a future reader doesn't tidy away a test that looks
like it's testing someone else's library for no reason. Tag as you see fit — `Engine_Components` /
`Phase2.7` seems right, but the tagging scheme is yours.

**3. Report the result back here, either way.** A failure isn't a problem to work around — it's a
design input, and it means I revise the spec's §3 rather than anyone routing around it in code.
Please don't try to make the interface approach work if it doesn't; just tell me.

**Not being asked:** anything about the broker, the interop assembly, or the ROM split. This ticket
is one question about NRules' matching semantics and nothing else.

### Response

**Spike test result: NRules does NOT support interface matching.**

Test: `NRulesInterfaceMatchingTests.cs` — define `IPlayerLocation` interface, concrete `PlayerPosition` implementer, rule with `Match<IPlayerLocation>()`, insert `new PlayerPosition(1)`, fire, assert marker exists. **Test FAILED** — marker was never inserted, meaning the rule never matched.

**Finding:** NRules' polymorphic matching (documented as `Match<Fruit>` matching `Apple`/`Pear`, `Match<object>` matching all facts) applies to **base class hierarchies only**, not interfaces. Concrete types match their base classes and ancestor classes, but NOT their interfaces.

**Why it matters:** This breaks the entire Phase 2.7 cross-ROM design as currently specified. The fallback Amara mentioned — sharing concrete fact types in an interop assembly instead of contracts — becomes necessary. This is worse architecturally (shared concrete types couple ROMs at implementation level, not just contract level), but it's what NRules actually supports.

### Addendum — round 2 (Amara, 2026-07-26)

Kept on this ticket rather than split: it's the same question — NRules' matching semantics — asked
about a different type relationship. Round 1's answer stands as reported.

**First, my error, not yours.** Your finding concludes that the shared-concrete-fact-type fallback
"becomes necessary." That follows correctly from what I wrote, but the fallback I named in the spec
was the wrong one to reach for, and I named it too quickly. There's a better option I skipped past:
an **abstract base class** contract. NRules' base-class polymorphism is the one thing here that *is*
documented and confirmed (`Match<Fruit>` matching `Apple` and `Pear`), so the contract can be an
abstract base rather than an interface:

```csharp
// interop assembly — the contract
public abstract class PlayerLocationFact
{
    public abstract int RoomId { get; }
}

// owning ROM — its own concrete fact, its own extra state
public sealed class PlayerPosition : PlayerLocationFact
{
    public override int RoomId { get; }
    public int MovesTaken { get; }      // stays off the contract
}

// consuming ROM
When().Match<PlayerLocationFact>(() => loc, loc => loc.RoomId == 3);
```

This keeps nearly everything the interface version bought — the owner keeps its concrete type and
private state, and the interop assembly holds only the contract rather than the whole fact. The cost
is C#'s single inheritance: a fact can carry exactly one contract, so a fact crossing two ROM
boundaries at once stops being expressible. That's a real narrowing, and acceptable for v1; it's not
a reason to prefer sharing concrete types.

**A methodological point on round 1, and it's a genuine question, not a criticism.** A passing test
proves a mechanism works. A *failing* test proves only that the rule didn't fire — which is
consistent with "NRules doesn't match interfaces," but equally consistent with the fixture not being
wired correctly, the rule not being loaded into the session, or the marker assertion being wrong. A
negative result needs a **positive control** before it can be trusted as a finding. So please confirm
round 1 with one: same fixture, same fact, same session, but the rule matching the **concrete**
`PlayerPosition` — if that fires and the interface version doesn't, the finding is solid and I'll
take it as settled. If the control *also* doesn't fire, the fixture is the problem and round 1 needs
re-running rather than believing. I'd rather spend ten minutes here than build a phase on a false
negative.

### Round 2 ask

1. **The core question:** does `Match<PlayerLocationFact>` fire on an inserted
   `PlayerPosition : PlayerLocationFact`?
2. **The positive control described above**, retro-fitted to round 1's interface case.
3. **Does a condition read through the base?** `Match<PlayerLocationFact>(() => loc, loc => loc.RoomId == 3)`
   must filter on the contract's members, not just match the type. A type match that can't carry a
   condition is useless to us.
4. **Can the owner still match its own concrete type?** This is the one I'd most expect to bite: ROM
   A's own rules match `PlayerPosition` while ROM B matches `PlayerLocationFact`, against **one
   inserted fact**. Both must fire. If inserting a fact only registers it under one type node, the
   whole design fails in a way none of the above would reveal.
5. **Two implementers, one contract** — two unrelated concrete facts extending the same base, both
   inserted; the consuming rule should fire once per fact.

Same standing request as round 1: **if it passes, keep it as a durable characterization test** rather
than deleting it, for the reason given above — this is third-party behaviour the entire ROM boundary
rests on. And same standing rule: **a failure is a design input, not something to engineer around.**
If abstract bases don't work either, tell me and I'll take the concrete-type fallback deliberately
rather than have anyone make it work quietly.

### Resolution

**Resolved 2026-07-26 (Amara, opener). Answered from source; the spike is set aside unfinished.**

**The evidence.** NRules is MIT-licensed and its API docs link every member to GitHub. Read at the
**v1.0.2 tag** — the version pinned in `LunaFlow.Engine.csproj` and `LunaFlow.Rules.csproj` — not
`main`, and not the prose docs:

```csharp
// Rete/TypeNode.cs
public override bool IsSatisfiedBy(IExecutionContext context, Fact fact)
{
    bool isMatchingType = FilterType.IsAssignableFrom(fact.FactType);
    return isMatchingType;
}

// Rete/RootNode.cs
public override bool IsSatisfiedBy(IExecutionContext context, Fact fact) => true;

// Rete/AlphaNode.cs — PropagateAssert offers every fact to every ChildNode,
// each filtering by its own IsSatisfiedBy. No exact-type dispatch table.
```

`Type.IsAssignableFrom` is true for interface implementation. Interfaces match. And because the root
admits everything and each type node filters independently, **one inserted fact satisfies every type
node it is assignable to** — the owner's concrete node and the peer's contract node both. That was
round 2's question 4, the one flagged as most likely to sink the design; it's fine.

**Round 1's result was a false negative** — the test genuinely failed, but not for the stated reason.
Nadia reported continued difficulty getting the fixture to run at all, which is consistent with a
harness problem rather than a library limitation (`RuleRepository.Load` silently skipping a rule
class that isn't public with a parameterless constructor is the usual culprit). Not chased down:
**Arryn's call — not worth pursuing until something actually fails.** Round 2 (abstract-base
contract) is withdrawn unbuilt; it existed only to work around a constraint that doesn't exist.

**Consequence for the spec:** [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md)
§3 stands unchanged, and §2's gate is lifted. The single-inheritance narrowing the fallback would
have imposed — one contract per crossing fact — does not apply. Contract types can be interfaces, so
a fact can cross more than one boundary.

**Accepted risk, disclosed rather than forgotten:** no test pins this NRules behaviour. The whole
cross-ROM boundary depends on `IsAssignableFrom` semantics in a third-party alpha network, and an
upgrade that narrowed polymorphic matching would surface as "a rule mysteriously stopped firing" a
long way from the cause. Taken knowingly, per above. *Revisit on any NRules version bump — that's the
trigger, not a date.*

**The actual lesson, which is process and not NRules.** This ran the wrong way round twice. First I
inferred library behaviour from prose docs; when that proved shaky I commissioned an experiment; the
experiment returned a false negative and I began redesigning around it. The authoritative answer was
freely available the entire time, in the dependency's own source, at the exact version we pin. A
spike answers "did my test pass," which is a different question from "how does this work," and it can
fail for reasons unrelated to what's being asked. Written up as a repo working convention in
[`CLAUDE.md`](../../../../CLAUDE.md) — *upstream source before inference* — so the next session
doesn't rediscover it. Kofi owns `CLAUDE.md`; added on Arryn's direct instruction and flagged for his
ratification or relocation.

One thing that did work and is worth keeping: **the positive control was requested before the source
was consulted**, on the general principle that a failing test proves only that the test failed. That
instinct was right, and it's what kept the false negative from being accepted as a finding.
