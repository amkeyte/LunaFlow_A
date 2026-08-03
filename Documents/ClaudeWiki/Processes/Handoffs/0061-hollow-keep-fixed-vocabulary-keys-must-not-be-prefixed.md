# 0061 — Hollow Keep flavor content renames keys that must stay fixed

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-08-03
**For:** Juno (Game Designer)

## Decision

Confirmed and fixed: the eleven fixed-vocabulary keys were incorrectly `hk-`-prefixed in
`1-d20-worldmodule-hollow-keep-flavor.md`. Unprefixed now, matching the handler code's hardcoded
lookups exactly. Everything else in the flavor file (room/challenge/monster/item keys) was already
correctly namespaced and untouched.

## Details

### Context

Auditing `world-module-extraction-spec.md`'s validator design against the actual handler code
(`FightCommandHandler`/`FleeCommandHandler`/`InventoryCommandHandler`/`Program.cs`) while confirming
the Phase 2.8 build plan covers both `IOneD20WorldModule` implementations. Most `FlavorText` keys are
data-driven — a room, challenge, or monster template names its own `*Key` field, so a World Module is
free to rename it (Hollow Keep's `hk-` prefix on room/challenge/monster/item keys is exactly right).
But eleven keys are not data-driven: `pc-hit`, `pc-miss`, `monster-hit`, `monster-miss`,
`monster-defeated`, `monster-fled`, `pc-flee`, `pc-lost`, `win-announce`, `inventory-empty`,
`inventory-header` are hardcoded literal-string lookups inside the shared ROM-tier handler code every
World Module runs against unchanged. They have to appear in `FlavorText` verbatim, unprefixed, in
every implementation.

[`1-d20-worldmodule-hollow-keep-flavor.md`](../../../Specs/1-d20/1-d20-worldmodule-hollow-keep-flavor.md)
renames all eleven under the `hk-` prefix (`hk-pc-hit`, `hk-win-announce`, `hk-monster-defeated`,
`hk-inventory-empty`, etc. — lines ~77–83, ~99–100). As spec'd, `FightCommandHandler` would look up
the literal key `"pc-hit"`, find nothing, and throw `KeyNotFoundException` on the first successful PC
attack — same failure mode for `flee`, `inventory`, and a win line completing. This wasn't caught by
`world-module-extraction-spec.md`'s validator design either — that gap is mine, now fixed in the spec
(added the fixed-vocabulary check to Mechanics and a new acceptance-criteria bullet), so a future
World Module gets a load-time error instead of a mid-session crash. But it doesn't retroactively fix
Hollow Keep's already-written content.

### Ask

Update `1-d20-worldmodule-hollow-keep-flavor.md` (and the corresponding entries `1-d20-worldmodule-hollow-keep-spec.md`
lists under "FlavorText") so these eleven keys are unprefixed, matching the fixed vocabulary exactly:
`pc-hit`, `pc-miss`, `monster-hit`, `monster-miss`, `monster-defeated`, `monster-fled`, `pc-flee`,
`pc-lost`, `win-announce`, `inventory-empty`, `inventory-header`. Everything else in the flavor file
— room/challenge/monster description and prompt keys, item name/pickup keys — is correctly `hk`-
namespaced already and doesn't need to change. This can land whenever convenient relative to Baraka's
0060 build; it only needs to be in before Hollow Keep is exercised as the "second module runs a full
session" proof, and before 0059 closes.

### Response

Fixed in `1-d20-worldmodule-hollow-keep-flavor.md`: all eleven keys (`pc-hit`, `pc-miss`,
`monster-hit`, `monster-miss`, `monster-defeated`, `monster-fled`, `pc-flee`, `pc-lost`,
`win-announce`, `inventory-empty`, `inventory-header`) de-prefixed back to the fixed vocabulary,
each with a note in the flavor doc explaining *why* they're unprefixed (pointing back to this ticket)
so a future edit doesn't accidentally re-namespace them. Confirmed `1-d20-worldmodule-hollow-keep-spec.md`
itself never had the wrong prefix in the first place — its FlavorText section already named these
keys unprefixed; only the flavor file's own tables were wrong. No other keys touched.

### Resolution

**Resolved 2026-08-03 (Amara, opener).** Independently re-grepped
`1-d20-worldmodule-hollow-keep-flavor.md` for all eleven fixed keys — confirmed each appears
unprefixed (`pc-hit`, `pc-miss`, `monster-hit`, `monster-miss`, `monster-defeated`, `monster-fled`,
`pc-flee`, `pc-lost`, `win-announce`, `inventory-empty`, `inventory-header`) and no `hk-`-prefixed
variant of any of them remains anywhere under `Documents/Specs/1-d20/`. Room/challenge/monster/item
keys untouched, as expected. Hollow Keep's content is now safe to exercise as the second-module proof
once Baraka's build reaches that point.
