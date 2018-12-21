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
            if (args.Length == 1)
            {
                return true;
            }
            return false;
        }
    }
}
