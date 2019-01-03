using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.Core;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        public ExitCommand() : base("exit", null, new string[] { "stop", "halt" })
        {}

        public override void RunCommand(string[] args)
        {
            Server.Instance.Stop();
        }

        public override bool VerifyArgs(string[] args)
        {
            return (args.Length == 0);
        }
    }
}
