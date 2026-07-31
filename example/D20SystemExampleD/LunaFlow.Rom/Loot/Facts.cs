namespace LunaFlow.Rom.Loot
{
    /// <summary>
    /// The monster-to-item drop table: 1:1, deterministic — same monster always drops the same
    /// item. Rulebook-authored, constructor-injected. Static content, per
    /// loot-rom-content-expansion-spec.md.
    /// </summary>
    public sealed record MonsterLootTable(IReadOnlyDictionary<string, string> ItemIdByMonsterId);

    /// <summary>
    /// Seeded once at session start, one per fight room, derived from that session's monster
    /// assignment. Not present for skill-check rooms; that absence is what keeps them loot-free.
    /// </summary>
    public sealed record LootAssignment(int RoomId, string ItemId);

    /// <summary>Fact marking that an item has been awarded for a room. Accumulates, never retracted.</summary>
    public sealed record ItemAwarded(string ItemId, int RoomId);

    /// <summary>
    /// The award rule's seam into Rulebook flavor text (ticket 0043's generalized pattern, applied
    /// here for its second use): already-resolved pickup text, not a key — generated
    /// <c>RulePrototype</c> code has no constructor/DI seam, so <c>Program.cs</c> resolves the
    /// flavor key once at seed time and hands the rule only the finished string, same pattern as
    /// D20System's <c>RoomChallengePrompt</c>/GridMap's <c>GameEndAnnouncement</c>. One per item,
    /// seeded at session start.
    /// </summary>
    public sealed record LootPickupText(string ItemId, string Text);
}
