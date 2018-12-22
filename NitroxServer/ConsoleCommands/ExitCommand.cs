using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        public ExitCommand()
        {
            Name = "exit";
        }

        public override void RunCommand(string[] args)
        {
            Server.Instance.Stop();
            Program.IsRunning = false;
        }

        public override bool VerifyArgs(string[] args)
        {
            return (args.Length == 0);
        }
    }
}
