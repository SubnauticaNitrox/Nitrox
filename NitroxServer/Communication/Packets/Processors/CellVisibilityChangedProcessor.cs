﻿using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
    {
        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;

        public CellVisibilityChangedProcessor(EntityManager entityManager, PlayerManager playerManager)
        {
            this.entityManager = entityManager;
            this.playerManager = playerManager;
        }

        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            SendNewlyVisibleEntities(player, packet.Added);

            List<OwnedGuid> ownershipChanges = new List<OwnedGuid>();
            AssignLoadedCellEntitySimulation(player, packet.Added, ownershipChanges);
            ReassignRemovedCellEntitySimulation(player, packet.Removed, ownershipChanges);
            BroadcastSimulationChanges(ownershipChanges);
        }

        private void SendNewlyVisibleEntities(Player player, VisibleCell[] visibleCells)
        {
            List<Entity> newlyVisibleEntities = entityManager.GetVisibleEntities(visibleCells);

            if (newlyVisibleEntities.Count > 0)
            {
                CellEntities cellEntities = new CellEntities(newlyVisibleEntities);
                player.SendPacket(cellEntities);
            }
        }

        private void AssignLoadedCellEntitySimulation(Player player, VisibleCell[] addedCells, List<OwnedGuid> ownershipChanges)
        {
            List<Entity> entities = entityManager.AssignEntitySimulation(player, addedCells);

            foreach (Entity entity in entities)
            {
                ownershipChanges.Add(new OwnedGuid(entity.Guid, player.Id, true));
            }
        }

        private void ReassignRemovedCellEntitySimulation(Player sendingPlayer, VisibleCell[] removedCells, List<OwnedGuid> ownershipChanges)
        {
            List<Entity> revokedEntities = entityManager.RevokeEntitySimulationFor(sendingPlayer, removedCells);

            foreach (Entity entity in revokedEntities)
            {
                VisibleCell entityCell = new VisibleCell(entity.Position, entity.Level);

                foreach (Player player in playerManager.GetPlayers())
                {
                    if (player != sendingPlayer && player.HasCellLoaded(entityCell))
                    {
                        Log.Info("player " + player.Id + " can take over " + entity.Guid);
                        ownershipChanges.Add(new OwnedGuid(entity.Guid, player.Id, true));
                    }
                }
            }
        }

        private void BroadcastSimulationChanges(List<OwnedGuid> ownershipChanges)
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
