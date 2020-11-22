using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityManager entityManager;
        private readonly EntitySimulation entitySimulation;

        public MovementPacketProcessor(EntityManager entityManager, EntitySimulation entitySimulation, PlayerManager playerManager)
        {
            this.entityManager = entityManager;
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;
        }

        public override void Process(Movement packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
            player.Transform.Position = packet.Position;

            CellChanges cellChanges = player.ProcessCellChanges();

            SendNewlyVisibleEntities(player, cellChanges);

            List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromCellSwitch(player, cellChanges);
            BroadcastSimulationChanges(ownershipChanges);
        }

        private void SendNewlyVisibleEntities(Player player, CellChanges cellChanges)
        {
            List<Entity> newlyVisibleEntities = entityManager.GetVisibleEntities(cellChanges);

            if (newlyVisibleEntities.Count > 0)
            {
                CellEntities cellEntities = new CellEntities(newlyVisibleEntities);
                player.SendPacket(cellEntities);
            }
        }

        private void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
        {
            if (ownershipChanges.Count > 0)
            {
                // TODO: This should be moved to `SimulationOwnership`
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }
        }
    }
}
