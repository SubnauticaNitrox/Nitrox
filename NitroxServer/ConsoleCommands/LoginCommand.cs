using System.Linq;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.ConfigParser;
using NitroxModel.Packets;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public LoginCommand(ServerConfig serverConfig) : base("login")
        {
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Player player)
        {
            if (args[0] == serverConfig.ServerAdminPassword)
            {
                player.isAdmin = true;
                Log.Info(player.Name + " logged in as Admin");
                player.SendPacket(new InGameMessageEvent("Successful Login"));

            }
            else
            {
                Log.Info(player.Name + " attempted to login as admin using " + args[0]);
                player.SendPacket(new InGameMessageEvent("Incorrect Password"));
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}

