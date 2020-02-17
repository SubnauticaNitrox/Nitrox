using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        private readonly PlayerManager playerManager;

        public OpCommand(PlayerManager playerManager) : base("op", Perms.ADMIN, "<name>", "Set an user as admin")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> runningPlayer)
        {
            string playerName = args[0];
            string message;

            Optional<Player> receivingPlayer = playerManager.GetPlayer(playerName);

            if(receivingPlayer.IsPresent())
            {
                receivingPlayer.Get().Permissions = Perms.ADMIN;
                message = $"Updated '{playerName}' permissions to admin";
            }
            else
            {
                message = $"Could not update permissions on unknown player '{playerName}'";
            }
            
            Log.Info(message);
            SendServerMessageIfPlayerIsPresent(runningPlayer, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
