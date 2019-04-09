using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class IpconfigCommand : Command
    {
        public IpconfigCommand() : base("ipconfig", Perms.CONSOLE, "", "Shows you all avaible IPs, you and your friends can connect.", new[] {"ips", "ip", "listip", "listips", "getip", "getips", "ports"})
        {
            //Hooooray! Command configured!
            //I have no idea, why did I write this here... Let's consider this as an easter egg for code rewiewers and contibutors.
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {

        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
