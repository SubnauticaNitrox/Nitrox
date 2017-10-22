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
        private TcpServer tcpServer;
        private EntitySpawner entitySpawner;

        public CellVisibilityChangedProcessor(TcpServer tcpServer, EntitySpawner entitySpawner)
        {
            this.tcpServer = tcpServer;
            this.entitySpawner = entitySpawner;
        }
        
        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            foreach (VisibleCell visibleCell in packet.Added)
            {
                List<SpawnedEntity> entities = entitySpawner.GetEntitiesByAbsoluteCell(visibleCell.AbsoluteCellEntity);

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
