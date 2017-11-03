using NitroxModel.DataStructures;
using NitroxModel.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System;
using System.Collections.Generic;

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

            entityManager.AssignEntitySimulation(player.Id, packet.Added);

            ReassignEntitySimulation(player, packet.Removed);
            SendNewlyVisibleEntities(player, packet.Added);
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

        private void ReassignEntitySimulation(Player sendingPlayer, VisibleCell[] removedCells)
        {
            List<Entity> revokedEntities = entityManager.RevokeEntitySimulationFor(sendingPlayer.Id, removedCells);
            List<OwnedGuid> newOwnerships = new List<OwnedGuid>();

            foreach (Entity entity in revokedEntities)
            {
                VisibleCell entityCell = new VisibleCell(entity.Position, entity.Level);

                foreach (Player player in playerManager.GetPlayers())
                {
                    if (player != sendingPlayer && player.HasCellLoaded(entityCell))
                    {
                        Console.Write("player " + player.Id + " can take over " + entity.Guid);
                        newOwnerships.Add(new OwnedGuid(entity.Guid, player.Id, true));
                    }
                }
            }

            if(newOwnerships.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(newOwnerships);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }
        }
    }
}
