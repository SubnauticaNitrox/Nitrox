using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

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
            player.AddChunks(packet.Added);
            player.RemoveChunks(packet.Removed);
        }
    }
}
