using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    internal class DeopCommand : Command
    {
        private readonly PlayerManager playerManager;

        public DeopCommand(PlayerManager playerManager) : base("deop", Perms.ADMIN, "<name>", "Remove admin rights from user")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> callingPlayer)
        {
            string playerName = args[0];
            string message;

            Optional<Player> targetPlayer = playerManager.GetPlayer(playerName);

            if (targetPlayer.IsPresent())
            {
                targetPlayer.Get().Permissions = Perms.PLAYER;
                message = "Updated " + playerName + " permissions to player";
            }
            else
            {
                message = "Could not update permissions on unknown player " + playerName;
            }

            Log.Info(message);
            SendServerMessageIfPlayerIsPresent(callingPlayer, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
