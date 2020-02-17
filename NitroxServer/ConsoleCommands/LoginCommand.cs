using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public LoginCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("login", Perms.PLAYER, "<password>")
        {
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            string pass = args[0];
            string message;

            if (pass == serverConfig.AdminPassword)
            {
                player.Get().Permissions = Perms.ADMIN;
                message = "Updated permissions to admin for " + player.Get().Name;
            }
            else
            {
                message = "Incorrect Password";
            }
            Log.Info(message);
            SendServerMessageIfPlayerIsPresent(player, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
