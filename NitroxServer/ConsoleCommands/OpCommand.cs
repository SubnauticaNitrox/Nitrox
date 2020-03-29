using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        private readonly PlayerManager playerManager;

        public OpCommand(PlayerManager playerManager) : base("op", Perms.ADMIN, "{name}", "Sets an user as admin")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            string playerName = args[0];
            string message;

            Optional<Player> receivingPlayer = playerManager.GetPlayer(playerName);

            if (receivingPlayer.HasValue)
            {
                receivingPlayer.Value.Permissions = Perms.ADMIN;
                message = $"Updated {playerName}\'s permissions to admin";
            }
            else
            {
                message = $"Could not update permissions of unknown player '{playerName}'";
            }

            Notify(sender, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
