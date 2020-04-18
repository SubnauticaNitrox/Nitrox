using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ExitCommand : Command
    {
        public ExitCommand() : base("stop", Perms.ADMIN, "Stops the server")
        {
            addAlias("exit", "halt", "quit");
        }

        public override void Perform(string[] args, Optional<Player> sender)
        {
            Server.Instance.Stop();
        }
    }
}
