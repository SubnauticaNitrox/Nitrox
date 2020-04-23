using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class ExitCommand : Command
    {
        public ExitCommand() : base("stop", Perms.ADMIN, "Stops the server")
        {
            AddAlias("exit", "halt", "quit");
        }

        protected override void Execute(CallArgs args)
        {
            Server.Instance.Stop();
        }
    }
}
