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
        private TcpServer tcpServer;
        private EntityManager entityManager;

        public CellVisibilityChangedProcessor(TcpServer tcpServer, EntityManager entityManager)
        {
            this.tcpServer = tcpServer;
            this.entityManager = entityManager;
        }
        
        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            entityManager.AllowEntitySimulationFor(player.Id, packet.Added);
            entityManager.RevokeEntitySimulationFor(player.Id, packet.Removed);

            List<SpawnedEntity> entities = entityManager.GetVisibleEntities(packet.Added);
            
            if (entities.Count > 0)
            {
                SpawnEntities spawnEntities = new SpawnEntities(entities);
                tcpServer.SendPacketToPlayer(spawnEntities, player);
                Console.WriteLine(spawnEntities);
            }
        }
    }
}
