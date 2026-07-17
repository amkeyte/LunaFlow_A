using LunaFlow.Engine.Game;

namespace LunaFlow.Engine.Commands
{
    public sealed class QuitCommandHandler : ICommandHandler
    {
        public void Handle(ICommand command)
        {
            Console.WriteLine("Shutting down...");
            GameEngineCore.Instance.RequestStop();
        }
    }
}
