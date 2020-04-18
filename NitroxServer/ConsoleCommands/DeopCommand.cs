using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class DeopCommand : Command
    {
        private readonly PlayerManager playerManager;

        public DeopCommand(PlayerManager playerManager) : base("deop", Perms.ADMIN, "Removes admin rights from user")
        {
            this.playerManager = playerManager;
            addParameter(null, TypePlayer.Get, "name", true);
        }

        public override void Perform(string[] args, Optional<Player> sender)
        {
            string playerName = args[0];
            string message;

            Optional<Player> targetPlayer = playerManager.GetPlayer(playerName);
            if (targetPlayer.HasValue)
            {
                targetPlayer.Value.Permissions = Perms.PLAYER;
                message = $"Updated {playerName}\'s permissions to PLAYER";
            }
            else
            {
                message = $"Could not update permissions of unknown player {playerName}";
            }

            SendMessageToBoth(sender, message);
        }
    }
}
