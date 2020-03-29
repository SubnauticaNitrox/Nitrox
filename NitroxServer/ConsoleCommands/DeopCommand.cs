using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class DeopCommand : Command
    {
        private readonly PlayerManager playerManager;

        public DeopCommand(PlayerManager playerManager) : base("deop", Perms.ADMIN, "{name}", "Removes admin rights from user")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
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

            Notify(sender, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
