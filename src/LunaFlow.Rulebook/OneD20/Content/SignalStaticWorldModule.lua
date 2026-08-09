-- "Signal Static" -- a second Department-7 mission, genuinely new content authored directly in Lua
-- (phase-2-12-lua-worldmodule-stress-test-spec.md items 9/10/11). No C# equivalent to diff against,
-- unlike DefaultOneD20WorldModule.lua's verbatim transcription. Same fixed 9-room grid topology as
-- every OneD20 World Module (Fight rooms 1, 5, 8; the rest SkillCheck) -- only the content filling
-- it is new. Fiction: a radio-relay station a Department-7 team is sent to secure and silence.

return {
  startRoomId = 5,

  rooms = {
    [1] = { row = 0, col = 0, challengeType = "Fight",      descriptionKey = "ss-room-1-desc" },
    [2] = { row = 0, col = 1, challengeType = "SkillCheck", descriptionKey = "ss-room-2-desc" },
    [3] = { row = 0, col = 2, challengeType = "SkillCheck", descriptionKey = "ss-room-3-desc" },
    [4] = { row = 1, col = 0, challengeType = "SkillCheck", descriptionKey = "ss-room-4-desc" },
    [5] = { row = 1, col = 1, challengeType = "Fight",      descriptionKey = "ss-room-5-desc" },
    [6] = { row = 1, col = 2, challengeType = "SkillCheck", descriptionKey = "ss-room-6-desc" },
    [7] = { row = 2, col = 0, challengeType = "SkillCheck", descriptionKey = "ss-room-7-desc" },
    [8] = { row = 2, col = 1, challengeType = "Fight",      descriptionKey = "ss-room-8-desc" },
    [9] = { row = 2, col = 2, challengeType = "SkillCheck", descriptionKey = "ss-room-9-desc" },
  },

  -- Item 9: a new pool of 9 skill/save challenges, own fiction throughout -- 7 skill-kind, 2
  -- save-kind (signal-arc/signal-haze), proving the Lua path round-trips Phase 2.10's Save-kind
  -- shape, not just the Phase 2.8 baseline.
  challenges = {
    ["signal-scan"] = { skill = "Search", skillAbility = "Int", dc = 8,
      successKey = "signal-scan-success", failureKey = "signal-scan-failure", promptKey = "signal-scan-prompt" },
    ["signal-climb"] = { skill = "Climb", skillAbility = "Str", dc = 10,
      successKey = "signal-climb-success", failureKey = "signal-climb-failure", promptKey = "signal-climb-prompt" },
    ["signal-listen"] = { skill = "Listen", skillAbility = "Wis", dc = 8,
      successKey = "signal-listen-success", failureKey = "signal-listen-failure", promptKey = "signal-listen-prompt" },
    ["signal-wade"] = { skill = "Swim", skillAbility = "Str", dc = 10,
      successKey = "signal-wade-success", failureKey = "signal-wade-failure", promptKey = "signal-wade-prompt" },
    ["signal-bypass"] = { skill = "Disable Device", skillAbility = "Int", dc = 12,
      successKey = "signal-bypass-success", failureKey = "signal-bypass-failure", promptKey = "signal-bypass-prompt" },
    ["signal-vault"] = { skill = "Jump", skillAbility = "Str", dc = 8,
      successKey = "signal-vault-success", failureKey = "signal-vault-failure", promptKey = "signal-vault-prompt" },
    ["signal-balance"] = { skill = "Balance", skillAbility = "Dex", dc = 8,
      successKey = "signal-balance-success", failureKey = "signal-balance-failure", promptKey = "signal-balance-prompt" },
    ["signal-arc"] = { kind = "save", saveType = "Reflex", dc = 10,
      successKey = "signal-arc-success", failureKey = "signal-arc-failure", promptKey = "signal-arc-prompt" },
    ["signal-haze"] = { kind = "save", saveType = "Fortitude", dc = 10,
      successKey = "signal-haze-success", failureKey = "signal-haze-failure", promptKey = "signal-haze-prompt" },
  },

  -- Item 10: procedural generation, not five hand-typed blocks -- the roster's stat progression is
  -- computed from a small base-plus-step table inside a loop, per the spec's own worked example.
  -- One monster (i == 1) gets item 11's Enrage fields (EnrageThresholdFraction = 0.5,
  -- EnrageDefenseBonus = 3, exactly as specced); one (i == 5) inflicts poison, proving the Lua path
  -- round-trips Phase 2.10's InflictsPoison shape too. Both are post-loop conditionals, not baked
  -- into the base-plus-step arithmetic itself, so the loop stays the arithmetic proof the spec asks
  -- for.
  monsters = (function()
    local monsterBase = { defense = 11, attackBonus = 2, maxHp = 6 }
    local monsters = {}
    for i = 1, 5 do
      local entry = {
        name = "Static Hound " .. i,
        descriptionKey = "ss-monster-" .. i .. "-desc",
        stats = {
          defense = monsterBase.defense + i,
          attackBonus = monsterBase.attackBonus,
          damageDie = "1d6",
          damageBonus = 0,
          maxHp = monsterBase.maxHp + (i * 2),
        },
      }
      if i == 1 then
        entry.enrageThresholdFraction = 0.5
        entry.enrageDefenseBonus = 3
      end
      if i == 5 then
        entry.inflictsPoison = true
      end
      monsters["signal-monster-" .. i] = entry
    end
    return monsters
  end)(),

  itemIdByMonsterId = {
    ["signal-monster-1"] = "antenna",
    ["signal-monster-2"] = "relay",
    ["signal-monster-3"] = "jammer",
    ["signal-monster-4"] = "dish",
    ["signal-monster-5"] = "transmitter",
  },

  -- The pregenerated PC's stats -- unchanged from the Strong-hero sheet, same reuse convention
  -- HollowKeepWorldModule already established for a second module.
  playerCharacter = { defense = 11, attackBonus = 4, damageDie = "1d8", damageBonus = 3, maxHp = 11 },
  playerAbilityScores = { Str = 16, Dex = 10, Con = 16, Int = 10, Wis = 10, Cha = 10 },

  flavorText = {
    -- Room descriptions.
    ["ss-room-1-desc"] = "The relay station's front gate, chain-link and rusted through. A dog paces behind it, ears already up.",
    ["ss-room-2-desc"] = "A gravel yard, transformer housings humming on either side.",
    ["ss-room-3-desc"] = "A maintenance shed, tools racked neat despite the dust.",
    ["ss-room-4-desc"] = "A drainage culvert, half-flooded, running under the access road.",
    ["ss-room-5-desc"] = "The relay station's main floor — antenna housings overhead, cable runs underfoot. This is where the signal actually gets made.",
    ["ss-room-6-desc"] = "A cramped equipment closet, breaker panels floor to ceiling.",
    ["ss-room-7-desc"] = "A catwalk strung between two antenna masts, wind cutting hard across it.",
    ["ss-room-8-desc"] = "The generator room, diesel fumes thick in the air and something moving in the shadow of the tanks.",
    ["ss-room-9-desc"] = "The station roof — the antenna array itself, close enough to touch.",

    -- Skill-check prompts/success/failure.
    ["signal-scan-prompt"] = "Something on this bench doesn't match the rest of the gear — worth a closer look (Search).",
    ["signal-scan-success"] = "You spot it — a logbook, hidden under a loose panel.",
    ["signal-scan-failure"] = "Nothing obvious yet — worth another look.",
    ["signal-climb-prompt"] = "The ladder up is bent halfway, but it might still hold (Climb).",
    ["signal-climb-success"] = "You get a grip and haul yourself up and over.",
    ["signal-climb-failure"] = "You lose your footing partway. No harm done — try again.",
    ["signal-listen-prompt"] = "Something's moving on the other side of the panel — better time it right (Listen).",
    ["signal-listen-success"] = "You catch the rhythm of it — you know when it's clear.",
    ["signal-listen-failure"] = "Hard to tell over the hum. Listen again.",
    ["signal-wade-prompt"] = "The culvert's flooded deeper than it looks — you'll have to wade it (Swim).",
    ["signal-wade-success"] = "You wade through — cold, but you make it across.",
    ["signal-wade-failure"] = "The current's stronger than it looks. You're pushed back to where you started.",
    ["signal-bypass-prompt"] = "The breaker panel's locked, and it looks worth getting into (Disable Device).",
    ["signal-bypass-success"] = "The panel gives with a click.",
    ["signal-bypass-failure"] = "Trickier than it looks. No damage done — try again.",
    ["signal-vault-prompt"] = "A gap in the catwalk grating, just wide enough to worry about (Jump).",
    ["signal-vault-success"] = "You clear the gap easily.",
    ["signal-vault-failure"] = "You pull up short at the edge. No harm — just try again.",
    ["signal-balance-prompt"] = "The roof's access strip is narrow, with a long drop either side (Balance).",
    ["signal-balance-success"] = "You cross without a wobble.",
    ["signal-balance-failure"] = "You lose your footing halfway — no fall, no harm, just start over.",
    ["signal-arc-prompt"] = "A cable's arcing where it shouldn't, whipping close enough to matter (dodge/brace).",
    ["signal-arc-success"] = "You throw yourself clear just in time.",
    ["signal-arc-failure"] = "It catches you — a jolt, nothing worse. You shake it off.",
    ["signal-haze-prompt"] = "Coolant fumes are thick here, catching in your throat (steady yourself).",
    ["signal-haze-success"] = "You get clear of the worst of it, coughing but steady.",
    ["signal-haze-failure"] = "It hits you harder than expected. You need a second to recover.",

    -- Monster arrival descriptions.
    ["ss-monster-1-desc"] = "A lean guard dog plants itself in the gateway, hackles up, growling low.",
    ["ss-monster-2-desc"] = "A second dog circles out from behind the transformer housing, quieter than the first.",
    ["ss-monster-3-desc"] = "A wiry mutt darts out from the equipment racks, teeth already bared.",
    ["ss-monster-4-desc"] = "A heavyset dog blocks the catwalk, refusing to give ground.",
    ["ss-monster-5-desc"] = "A scarred dog slinks out from beside the generator, favoring one side — this one bites and doesn't let go.",

    -- Combat narration (fixed vocabulary — read literally by FightCommandHandler/FleeCommandHandler
    -- regardless of which World Module is active).
    ["monster-defeated"] = "It goes down and stays down.",
    ["monster-fled"] = "It backs off into the dark before you can finish it — no catching it now.",
    ["pc-hit"] = "Your strike connects — it yelps but it's still on its feet.",
    ["pc-miss"] = "You swing and miss.",
    ["monster-hit"] = "It gets its teeth into you — that's going to need looking at.",
    ["monster-miss"] = "It lunges and you twist clear.",
    ["pc-flee"] = "You break off and put distance between you and it. It doesn't follow.",
    ["pc-lost"] = "It gets the better of you this time — you're forced back, bitten. No going through it, not today.",
    ["win-announce"] = "The relay's dead quiet now. That's the job done.",

    -- Item 11: Enrage announcement — read only for a monster with EnrageThresholdFraction set
    -- (signal-monster-1 here); not part of the fixed-vocabulary list since most World Modules never
    -- trigger this code path at all.
    ["monster-enraged"] = "Bloodied, it stops circling and plants its feet — it's not backing off now.",

    -- Loot — item names and pickup text.
    ["item-antenna-name"] = "Directional Antenna",
    ["item-antenna-pickup"] = "You pull the antenna module free — still intact.",
    ["item-relay-name"] = "Signal Relay",
    ["item-relay-pickup"] = "You pocket the relay board before it can be smashed.",
    ["item-jammer-name"] = "Pocket Jammer",
    ["item-jammer-pickup"] = "You take the jammer off it — small, but it'll do something.",
    ["item-dish-name"] = "Folding Dish",
    ["item-dish-pickup"] = "You collapse the dish and stow it.",
    ["item-transmitter-name"] = "Backup Transmitter",
    ["item-transmitter-pickup"] = "You take the transmitter — battered, but the light's still on.",

    -- Loot — Phase 2.11 item 7's fixed bonus item, granted once the party reaches Veteran-or-above
    -- reputation. Not part of the monster-drop table; every World Module a full session might run
    -- against needs this key, since Program.cs seeds it unconditionally.
    ["item-bonus-charm-name"] = "Loyalty Charm",
    ["item-bonus-charm-pickup"] = "Word's gotten around about what you can do — that's worth something extra. You're handed a loyalty charm.",

    -- Loot — inventory command text.
    ["inventory-empty"] = "You're not carrying anything yet.",
    ["inventory-header"] = "You're carrying:",
  },
}
