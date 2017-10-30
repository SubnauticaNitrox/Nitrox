using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private readonly TcpServer tcpServer;

        public MovementPacketProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void Process(Movement packet, Player player)
        {
            player.Position = packet.Position;
            tcpServer.SendPacketToOtherPlayers(packet, player);
        }
    }
}
