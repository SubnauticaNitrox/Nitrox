using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private TcpServer tcpServer;

        public MovementPacketProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }
        
        public override void Process(Movement packet, Player player)
        {
            player.Position = packet.PlayerPosition;
            tcpServer.SendPacketToOtherPlayers(packet, player);
        }
    }
}
