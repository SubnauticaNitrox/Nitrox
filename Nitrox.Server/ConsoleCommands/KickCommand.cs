using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Entities;

namespace Nitrox.Server.ConsoleCommands
{
    internal class KickCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public KickCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("kick", Perms.ADMIN, "Kicks a player from the server", true)
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypeString("reason", false));
        }

        protected override void Execute(CallArgs args)
        {
            Player playerToKick = args.Get<Player>(0);

            playerToKick.SendPacket(new PlayerKicked($"You were kicked from the server ! \n Reason : {args.GetTillEnd(1)}"));
            playerManager.PlayerDisconnected(playerToKick.connection);

            List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);
            if (revokedEntities.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }

            playerManager.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
            SendMessage(args.Sender, $"The player {playerToKick.Name} has been disconnected");
        }
    }
}
