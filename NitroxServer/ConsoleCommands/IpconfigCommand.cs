using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class IpconfigCommand : Command
    {
        public IpconfigCommand() : base("ipconfig", Optional<string>.Empty(), "Shows posible IPs to join the server.",  new [] {"ip", "ips", "join", "joinables" })
        {
        }

        public override void RunCommand(string[] args)
        {
            IpLogger.PrintServerIps();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
