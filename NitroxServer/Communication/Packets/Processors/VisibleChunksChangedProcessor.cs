using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Threading;
using NitroxServer.GameLogic.Threading.Actions;
using System;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors
{
    class VisibleChunksChangedProcessor : AuthenticatedPacketProcessor<VisibleChunksChanged>
    {
        private TcpServer tcpServer;
        private GameActionManager gameActionManager;

        public VisibleChunksChangedProcessor(TcpServer tcpServer, GameActionManager gameActionManager)
        {
            this.tcpServer = tcpServer;
            this.gameActionManager = gameActionManager;
        }
        
        public override void Process(VisibleChunksChanged packet, Player player)
        {
            HashSet<Int3> add = new HashSet<Int3>();

            foreach(var modelInt3 in packet.Added)
            {
                Int3 chunk = new Int3(modelInt3.X, modelInt3.Y, modelInt3.Z);
                add.Add(chunk);
                gameActionManager.add(new LoadChunkAction(chunk));
            }

            HashSet<Int3> removed = new HashSet<Int3>();

            foreach (var modelInt3 in packet.Removed)
            {
                Int3 chunk = new Int3(modelInt3.X, modelInt3.Y, modelInt3.Z);
                add.Add(chunk);
                gameActionManager.add(new UnloadChunkAction(chunk));
            }

            player.AddChunks(add);
            player.RemoveChunks(removed);
        }
    }
}
