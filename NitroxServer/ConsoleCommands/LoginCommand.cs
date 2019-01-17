using System.Text.RegularExpressions;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Players;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;

namespace NitroxServer.ConsoleCommands
{
    class LoginCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public LoginCommand(PlayerData playerData, PlayerManager playerManager, ServerConfig serverConfig) : base("login", Perms.PLAYER, "<password>")
        {
            this.playerData = playerData;
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            string pass = args[0];
            string message;
            string playerName = player.Get().Name;

            if (pass == serverConfig.ServerAdminPassword)
            {
                if (playerData.UpdatePlayerPermissions(playerName, Perms.ADMIN))
                {
                    message = "Updated permissions to admin";
                }
                else
                {
                    message = "Could not update permissions " + player.ToString();
                }
            }else
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
