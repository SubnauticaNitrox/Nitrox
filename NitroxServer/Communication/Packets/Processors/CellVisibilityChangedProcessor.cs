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

        public CellVisibilityChangedProcessor(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }
        
        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            entityManager.AllowEntitySimulationFor(player.Id, packet.Added);
            entityManager.RevokeEntitySimulationFor(player.Id, packet.Removed);

            List<Entity> entities = entityManager.GetVisibleEntities(packet.Added);
            
            if (entities.Count > 0)
            {
                CellEntities cellEntities = new CellEntities(entities);
                player.SendPacket(cellEntities);
                Console.WriteLine(cellEntities);
            }
        }
    }
}
