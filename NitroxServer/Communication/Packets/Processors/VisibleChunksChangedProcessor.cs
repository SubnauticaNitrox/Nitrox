using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Threading;
using System;

namespace NitroxServer.Communication.Packets.Processors
{
    class VisibleChunksChangedProcessor : AuthenticatedPacketProcessor<VisibleChunksChanged>
    {
        private TcpServer tcpServer;
        private GameActionManager gameActionManager;
        private ChunkManager chunkManager;

        public VisibleChunksChangedProcessor(TcpServer tcpServer, GameActionManager gameActionManager, ChunkManager chunkManager)
        {
            this.tcpServer = tcpServer;
            this.gameActionManager = gameActionManager;
            this.chunkManager = chunkManager;
        }
        
        public override void Process(VisibleChunksChanged packet, Player player)
        {
            player.AddChunks(packet.Added);
            player.RemoveChunks(packet.Removed);
            
            foreach(Chunk chunk in packet.Added)
            {
                chunkManager.PlayerEnteredChunk(chunk);
            }
            
            foreach (Chunk chunk in packet.Removed)
            {
                chunkManager.PlayerLeftChunk(chunk);
            }
        }
    }
}
