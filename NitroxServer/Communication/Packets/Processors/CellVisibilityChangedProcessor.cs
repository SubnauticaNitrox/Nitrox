using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
    {
        private readonly EntityManager entityManager;
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        private static int simCount = 0;
        private static int cellCount = 0;

        public CellVisibilityChangedProcessor(EntityManager entityManager, EntitySimulation entitySimulation, PlayerManager playerManager)
        {
            this.entityManager = entityManager;
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;
        }

        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            SendNewlyVisibleEntities(player, packet.Added);

            List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromCellSwitch(player, packet.Added, packet.Removed);
            BroadcastSimulationChanges(ownershipChanges);
        }

        private void SendNewlyVisibleEntities(Player player, AbsoluteEntityCell[] visibleCells)
        {
            List<Entity> newlyVisibleEntities = entityManager.GetVisibleEntities(visibleCells);

            if (newlyVisibleEntities.Count > 0)
            {
                CellEntities cellEntities = new CellEntities(newlyVisibleEntities);
                cellEntities.Count = cellCount;
                cellCount++;
                player.SendPacket(cellEntities);
            }
        }

        private void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
        {
            if (ownershipChanges.Count > 0)
            {
                // TODO: This should be moved to `SimulationOwnership`
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges);
                ownershipChange.Count = simCount;
                simCount++;
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }
        }
    }
}
