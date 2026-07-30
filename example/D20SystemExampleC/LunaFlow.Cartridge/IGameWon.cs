namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract marker for cross-ROM fact: the game has been won. No members — <c>Not&lt;T&gt;()</c>
    /// only needs the type to exist in working memory, never a property read, so there's nothing to
    /// carry across (not even <c>WinMarker</c>'s own <c>Line</c>).
    /// </summary>
    public interface IGameWon
    {
    }
}
