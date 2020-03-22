using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ExitCommand : Command
    {
        public ExitCommand() : base("stop", Perms.ADMIN, "", "Stops the server", new[] { "exit", "halt", "quit" })
        {
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            Server.Instance.Stop();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
