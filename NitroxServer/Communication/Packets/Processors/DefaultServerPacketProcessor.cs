using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<AuthenticatedPacket>
    {
        private TcpServer tcpServer;

        private HashSet<Type> loggingPacketBlackList = new HashSet<Type> {
            typeof(AnimationChangeEvent),
            typeof(Movement),
            typeof(VehicleMovement),
            typeof(ItemPosition)
        };

        public DefaultServerPacketProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void Process(AuthenticatedPacket packet, Player player)
        {
            if (!loggingPacketBlackList.Contains(packet.GetType()))
            {
                Console.WriteLine("Using default packet processor for: " + packet.ToString() + " and player " + player.Id);
            }

            tcpServer.SendPacketToOtherPlayers(packet, player);
        }
    }
}
