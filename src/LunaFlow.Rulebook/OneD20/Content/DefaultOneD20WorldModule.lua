-- Verbatim Lua transcription of DefaultOneD20WorldModule.cs and
-- DefaultOneD20WorldModule.FlavorText.cs's shipped content. No new game content, no
-- creative changes -- a faithful format conversion only. See
-- Documents/Specs/lua-world-module-spec.md's "Type vocabulary" section for the target shape.

return {
  startRoomId = 5,

  -- The nine rooms' fixed grid position and slot-kind. Rooms 1, 5, 8 are Fight; the rest
  -- are SkillCheck -- this topology never varies per session.
  rooms = {
    [1] = { row = 0, col = 0, challengeType = "Fight",      descriptionKey = "room-1-desc" },
    [2] = { row = 0, col = 1, challengeType = "SkillCheck", descriptionKey = "room-2-desc" },
    [3] = { row = 0, col = 2, challengeType = "SkillCheck", descriptionKey = "room-3-desc" },
    [4] = { row = 1, col = 0, challengeType = "SkillCheck", descriptionKey = "room-4-desc" },
    [5] = { row = 1, col = 1, challengeType = "Fight",      descriptionKey = "room-5-desc" },
    [6] = { row = 1, col = 2, challengeType = "SkillCheck", descriptionKey = "room-6-desc" },
    [7] = { row = 2, col = 0, challengeType = "SkillCheck", descriptionKey = "room-7-desc" },
    [8] = { row = 2, col = 1, challengeType = "Fight",      descriptionKey = "room-8-desc" },
    [9] = { row = 2, col = 2, challengeType = "SkillCheck", descriptionKey = "room-9-desc" },
  },

  -- The pool of 9 skill-check challenges. Outer table key IS the challenge's Key -- not
  -- repeated as a field inside the entry.
  challenges = {
    search = { skill = "Search", skillAbility = "Int", dc = 10,
               successKey = "search-success", failureKey = "search-failure",
               promptKey = "search-prompt" },
    climb = { skill = "Climb", skillAbility = "Str", dc = 12,
              successKey = "climb-success", failureKey = "climb-failure",
              promptKey = "climb-prompt" },
    listen = { skill = "Listen", skillAbility = "Wis", dc = 8,
               successKey = "listen-success", failureKey = "listen-failure",
               promptKey = "listen-prompt" },
    swim = { skill = "Swim", skillAbility = "Str", dc = 10,
             successKey = "swim-success", failureKey = "swim-failure",
             promptKey = "swim-prompt" },
    disabledevice = { skill = "Disable Device", skillAbility = "Int", dc = 12,
                       successKey = "disabledevice-success", failureKey = "disabledevice-failure",
                       promptKey = "disabledevice-prompt" },
    jump = { skill = "Jump", skillAbility = "Str", dc = 8,
             successKey = "jump-success", failureKey = "jump-failure",
             promptKey = "jump-prompt" },
    balance = { skill = "Balance", skillAbility = "Dex", dc = 11,
                successKey = "balance-success", failureKey = "balance-failure",
                promptKey = "balance-prompt" },
    spot = { skill = "Spot", skillAbility = "Wis", dc = 13,
             successKey = "spot-success", failureKey = "spot-failure",
             promptKey = "spot-prompt" },
    concentration = { skill = "Concentration", skillAbility = "Con", dc = 12,
                       successKey = "concentration-success", failureKey = "concentration-failure",
                       promptKey = "concentration-prompt" },
    -- Save-kind additions (phase-2-10-content-coverage-spec.md). No skill/skillAbility;
    -- kind/saveType instead.
    wire = { kind = "save", saveType = "Reflex", dc = 10,
             successKey = "wire-success", failureKey = "wire-failure",
             promptKey = "wire-prompt" },
    fumes = { kind = "save", saveType = "Fortitude", dc = 12,
              successKey = "fumes-success", failureKey = "fumes-failure",
              promptKey = "fumes-prompt" },
  },

  -- The roster of 5 monsters. Outer table key IS the monster's Id -- not repeated as a
  -- field inside the entry.
  monsters = {
    duelist = { name = "The Duelist", descriptionKey = "monster-duelist-desc",
      stats = { defense = 13, attackBonus = 3, damageDie = "1d6", damageBonus = 1, maxHp = 8 } },
    brawler = { name = "The Brawler", descriptionKey = "monster-brawler-desc",
      stats = { defense = 11, attackBonus = 4, damageDie = "1d4", damageBonus = 3, maxHp = 12 } },
    marksman = { name = "The Marksman", descriptionKey = "monster-marksman-desc",
      stats = { defense = 12, attackBonus = 5, damageDie = "2d6", damageBonus = 0, maxHp = 6 } },
    enforcer = { name = "The Enforcer", descriptionKey = "monster-enforcer-desc",
      stats = { defense = 15, attackBonus = 2, damageDie = "1d8", damageBonus = 2, maxHp = 14 } },
    scrapper = { name = "The Scrapper", descriptionKey = "monster-scrapper-desc",
      stats = { defense = 14, attackBonus = 2, damageDie = "1d4", damageBonus = 1, maxHp = 6 },
      inflictsPoison = true },
  },

  -- The static, deterministic monster->item drop table (1:1).
  itemIdByMonsterId = {
    duelist = "rapier",
    brawler = "knuckles",
    marksman = "pistol",
    enforcer = "vest",
    scrapper = "knife",
  },

  -- The pregenerated level-1 Strong-hero PC's stats.
  playerCharacter = { defense = 11, attackBonus = 4, damageDie = "1d8", damageBonus = 3, maxHp = 11 },

  -- The PC's six ability scores, per the Strong-hero sheet.
  playerAbilityScores = { Str = 16, Dex = 10, Con = 16, Int = 10, Wis = 10, Cha = 10 },

  -- Flavor text, keyed identically to DefaultOneD20WorldModule.FlavorText.cs's FlavorText
  -- dictionary. Every string copied byte-for-byte -- no rewording, no retranslation.
  flavorText = {
    -- Room descriptions -- unchanged from 1-d20-flavor.md; room position/topology never varies per session.
    ["room-1-desc"] = "Crates stacked head-high, a rolled-up delivery door letting in a slice of grey daylight. Someone's still here.",
    ["room-2-desc"] = "Shelves of unlabeled boxes, floor to ceiling.",
    ["room-3-desc"] = "A chain-link fence, taller than it looked from outside, cuts across the yard.",
    ["room-4-desc"] = "A narrow hallway with a door at the far end, slightly ajar.",
    ["room-5-desc"] = "The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.",
    ["room-6-desc"] = "A burst pipe has turned this stretch of hallway into a shin-deep wading pool.",
    ["room-7-desc"] = "A cramped office, papers everywhere.",
    ["room-8-desc"] = "A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above.",
    ["room-9-desc"] = "A metal catwalk with a section of grating torn away.",

    -- Skill-check prompts (challenge-indexed -- a challenge can now land on any room of its type).
    ["search-prompt"] = "Something here feels arranged, not left to chance — worth a closer look (Search).",
    ["climb-prompt"] = "There's a way up, but it won't be easy (Climb).",
    ["listen-prompt"] = "Someone's close by — better time it right (Listen).",
    ["swim-prompt"] = "There's water blocking the way — you'll have to get through it (Swim).",
    ["disabledevice-prompt"] = "Something's locked up tight, and it looks worth getting into (Disable Device).",
    ["jump-prompt"] = "A gap just wide enough to worry about (Jump).",
    ["balance-prompt"] = "There's a way across, if you can keep your footing (Balance).",
    ["spot-prompt"] = "Something here doesn't look right — worth a closer look (Spot).",
    ["concentration-prompt"] = "This is going to take steady focus, and it won't be easy to keep (Concentration).",

    -- Skill-check success/failure (challenge-indexed).
    ["search-success"] = "You spot the latch, cleverly hidden nearby.",
    ["search-failure"] = "Nothing obvious yet — worth another look.",
    ["climb-success"] = "You get a grip and haul yourself up and over.",
    ["climb-failure"] = "You lose your footing partway. No harm done — try again.",
    ["listen-success"] = "You catch the rhythm of their movement — you know when it's clear.",
    ["listen-failure"] = "Hard to tell over the noise. Listen again.",
    ["swim-success"] = "You wade through — cold, but you make it across.",
    ["swim-failure"] = "The current's stronger than it looks. You're pushed back to where you started.",
    ["disabledevice-success"] = "The lock gives with a click.",
    ["disabledevice-failure"] = "Trickier than it looks. No damage done — try again.",
    ["jump-success"] = "You clear the gap easily.",
    ["jump-failure"] = "You pull up short at the edge. No harm — just try again.",
    ["balance-success"] = "You cross without a wobble.",
    ["balance-failure"] = "You lose your footing halfway — no fall, no harm, just start over.",
    ["spot-success"] = "There it is — you spot what you were missing.",
    ["spot-failure"] = "Nothing catches your eye yet. Look again.",
    ["concentration-success"] = "You block out the distraction and get it done.",
    ["concentration-failure"] = "Your focus slips at the wrong moment. Try again.",

    -- Save-check prompts/success/failure (phase-2-10-content-coverage-spec.md).
    ["wire-prompt"] = "A live wire's come loose and it's whipping around close enough to matter (dodge/brace).",
    ["wire-success"] = "You throw yourself clear just in time.",
    ["wire-failure"] = "It catches you — a jolt, nothing worse. You shake it off.",
    ["fumes-prompt"] = "The air's thick with dust and something chemical — it's getting hard to breathe (steady yourself).",
    ["fumes-success"] = "You get clear of the worst of it, coughing but steady.",
    ["fumes-failure"] = "It hits you harder than expected. You need a second to recover.",

    -- Monster arrival descriptions (monster-indexed -- replaces the single fixed monster-desc key).
    ["monster-duelist-desc"] = "A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk.",
    ["monster-brawler-desc"] = "A thick-necked man cracks his knuckles and squares up, grinning like this is the part of his day he enjoys.",
    ["monster-marksman-desc"] = "A lean figure levels a pistol at you without a word of warning.",
    ["monster-enforcer-desc"] = "A heavyset man in a reinforced vest plants himself in your way, arms crossed, daring you to try something.",
    ["monster-scrapper-desc"] = "A wiry kid flicks open a knife, more nervous than dangerous, but not backing down either.",

    -- Combat narration -- monster-defeated/monster-hit reworded to drop weapon-specific bleed
    -- (the original text named "the rapier" specifically); the rest unchanged.
    ["monster-defeated"] = "He goes down and doesn't get up.",
    ["monster-fled"] = "He slips past you and out of sight before you can stop him — no catching him now.",
    ["pc-hit"] = "Your sword connects — he staggers but he's still on his feet.",
    ["pc-miss"] = "You swing and miss.",
    ["monster-hit"] = "He gets a hit in — that's going to leave a mark.",
    ["monster-miss"] = "He comes at you and you dodge clear.",
    ["pc-flee"] = "You break off and put the door between you and him. He doesn't follow.",
    ["pc-lost"] = "He gets the better of you this time — you're forced back, bruised. No going through him, not today.",
    ["win-announce"] = "Three for three — that's enough to call it. Mission complete.",

    -- Loot -- item names and pickup text.
    ["item-rapier-name"] = "The Duelist's Rapier",
    ["item-rapier-pickup"] = "You take his rapier — well-balanced, well cared for.",
    ["item-knuckles-name"] = "Brass Knuckles",
    ["item-knuckles-pickup"] = "You pocket a pair of brass knuckles, still warm.",
    ["item-pistol-name"] = "Backup Piece",
    ["item-pistol-pickup"] = "You take the spare pistol from his jacket.",
    ["item-vest-name"] = "Kevlar Vest",
    ["item-vest-pickup"] = "You strip the vest off him — scuffed, but solid.",
    ["item-knife-name"] = "Folding Knife",
    ["item-knife-pickup"] = "You take the knife from his hand before he can grab it back.",

    -- Loot -- phase-2-11-capability-stress-test-spec.md item 7's fixed bonus item, granted once
    -- the party reaches Veteran-or-above reputation. Not part of the monster-drop table.
    ["item-bonus-charm-name"] = "Loyalty Charm",
    ["item-bonus-charm-pickup"] = "Word's gotten around about what you can do — that's worth something extra. You're handed a loyalty charm.",

    -- Loot -- inventory command text.
    ["inventory-empty"] = "You're not carrying anything yet.",
    ["inventory-header"] = "You're carrying:",
  },
}
