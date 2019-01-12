using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        public ExitCommand() : base("exit", Perms.ADMIN, "", "Exits the server", new[] {"stop", "halt", "quit", "abort"})
        {
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            Server.Instance.Stop();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
