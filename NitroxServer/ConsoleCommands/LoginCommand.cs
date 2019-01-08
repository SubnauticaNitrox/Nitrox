using System.Linq;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.ConfigParser;

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
            }
            else
            {
                Log.Info(player.Name + " attempted to login as admin using " + args[0]);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}

