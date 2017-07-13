using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType;
        private Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType;

        private DefaultServerPacketProcessor defaultPacketProcessor;

        public PacketHandler(TcpServer tcpServer, TimeKeeper timeKeeper)
        {
            this.defaultPacketProcessor = new DefaultServerPacketProcessor(tcpServer);

            this.authenticatedPacketProcessorsByType = new Dictionary<Type, PacketProcessor>() {
                {typeof(Movement), new MovementPacketProcessor(tcpServer) },
            };

            this.unauthenticatedPacketProcessorsByType = new Dictionary<Type, PacketProcessor>() {
                {typeof(Authenticate), new AuthenticatePacketProcessor(tcpServer, timeKeeper) }
            };
        }

        public void ProcessAuthenticated(Packet packet, Player player)
        {
            if (authenticatedPacketProcessorsByType.ContainsKey(packet.GetType()))
            {
                authenticatedPacketProcessorsByType[packet.GetType()].ProcessPacket(packet, player);
            }
            else
            {
                defaultPacketProcessor.ProcessPacket(packet, player);
            }
        }

        public void ProcessUnauthenticated(Packet packet, Connection connection)
        {
            Validate.IsFalse(packet is AuthenticatedPacket);

            if (unauthenticatedPacketProcessorsByType.ContainsKey(packet.GetType()))
            {
                unauthenticatedPacketProcessorsByType[packet.GetType()].ProcessPacket(packet, connection);
            }
            else
            {
                Console.WriteLine("Received invalid, unauthenticated packet: " + packet);
            }
        }
    }
}
