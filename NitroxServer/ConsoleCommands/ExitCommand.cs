using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        public ExitCommand() : base("exit", Optional<string>.Empty(), "Exits the server", new[] {"stop", "halt", "quit", "abort"})
        {
        }

        public override void RunCommand(string[] args)
        {
            Server.Instance.Stop();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
