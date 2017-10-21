using NitroxModel.DataStructures;
using NitroxModel.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors
{
    class VisibleChunksChangedProcessor : AuthenticatedPacketProcessor<VisibleChunksChanged>
    {
        private TcpServer tcpServer;
        private EntitySpawner entitySpawner;

        public VisibleChunksChangedProcessor(TcpServer tcpServer, EntitySpawner entitySpawner)
        {
            this.tcpServer = tcpServer;
            this.entitySpawner = entitySpawner;
        }
        
        public override void Process(VisibleChunksChanged packet, Player player)
        {
            player.AddChunks(packet.Added);
            player.RemoveChunks(packet.Removed);

            foreach (Chunk visibleChunk in packet.Added)
            {
                Int3 absoluteCellId = EntityCellHelper.GetAbsoluteCellId(visibleChunk.BatchId, visibleChunk.CellId);
                List<SpawnedEntity> entities = entitySpawner.GetEntitiesByAbsoluteCellId(absoluteCellId);

                if(entities.Count > 0)
                {
                    SpawnEntities spawnEntities = new SpawnEntities(entities);
                    tcpServer.SendPacketToPlayer(spawnEntities, player);
                    Console.WriteLine(spawnEntities);
                }
            }
        }
    }
}
