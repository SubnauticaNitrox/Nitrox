using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors
{
    class VisibleChunksChangedProcessor : AuthenticatedPacketProcessor<VisibleChunksChanged>
    {
        private TcpServer tcpServer;

        public VisibleChunksChangedProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }
        
        public override void Process(VisibleChunksChanged packet, Player player)
        {
            Console.WriteLine(packet);

            HashSet<Int3> add = new HashSet<Int3>();

            foreach(var modelInt3 in packet.Added)
            {
                Int3 chunk = new Int3(modelInt3.X, modelInt3.Y, modelInt3.Z);
                add.Add(chunk);
            }

            HashSet<Int3> removed = new HashSet<Int3>();

            foreach (var modelInt3 in packet.Removed)
            {
                Int3 chunk = new Int3(modelInt3.X, modelInt3.Y, modelInt3.Z);
                add.Add(chunk);
            }

            player.AddChunks(add);
            player.RemoveChunks(removed);
        }
    }
}
