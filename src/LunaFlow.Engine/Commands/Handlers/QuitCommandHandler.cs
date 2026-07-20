using LunaFlow.Engine.Game;

namespace LunaFlow.Engine.Commands
{
    /// <summary>
    /// Handles the <c>quit</c>/<c>exit</c> command by requesting the game engine loop stop.
    /// </summary>
    public sealed class QuitCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Requests that <see cref="GameEngineCore"/> stop its run loop.
        /// </summary>
        public void Handle(ICommand command)
        {
            Console.WriteLine("Shutting down...");
            GameEngineCore.Instance.RequestStop();
        }
    }
}
