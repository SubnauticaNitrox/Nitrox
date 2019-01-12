using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Players;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    class DeopCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;

        public DeopCommand(PlayerData playerData, PlayerManager playerManager) : base("deop", Perms.ADMIN, "<name>")
        {
            this.playerData = playerData;
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            string playerName = args[0];
            string message;

            if (playerData.UpdatePlayerPermissions(playerName, Perms.PLAYER))
            {
                message = "Updated " + playerName + " permissions to player";
            }
            else
            {
                message = "Could not update permissions on unknown player " + playerName;
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
