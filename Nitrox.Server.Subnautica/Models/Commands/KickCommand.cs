using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class KickCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public KickCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("kick", Perms.MODERATOR, "Kicks a player from the server")
        {
            AddParameter(new TypePlayer("name", true, "Name of the player to kick"));
            AddParameter(new TypeString("reason", false, "Reason for kicking the player"));

            AllowedArgOverflow = true;

            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
        }

        protected override void Execute(CallArgs args)
        {
            Player playerToKick = args.Get<Player>(0);

            if (args.SenderName == playerToKick.Name)
            {
                SendMessage(args.Sender, "You can't kick yourself");
                return;
            }

            if (!args.IsConsole && playerToKick.Permissions >= args.Sender.Value.Permissions)
            {
                SendMessage(args.Sender, $"You're not allowed to kick {playerToKick.Name}");
                return;
            }

            playerToKick.SendPacket(new PlayerKicked(args.GetTillEnd(1)));
            playerManager.PlayerDisconnected(playerToKick.Connection);

            List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);
            if (revokedEntities.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new(revokedEntities);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }

            playerManager.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
            SendMessage(args.Sender, $"The player {playerToKick.Name} has been disconnected");
        }
    }
}
