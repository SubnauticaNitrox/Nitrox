﻿using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
    {
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public CellVisibilityChangedProcessor(WorldEntityManager worldEntityManager, EntitySimulation entitySimulation, PlayerManager playerManager)
        {
            this.worldEntityManager = worldEntityManager;
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
            List<WorldEntity> newlyVisibleEntities = worldEntityManager.GetVisibleEntities(visibleCells);

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
