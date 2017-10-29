using System;
using System.Collections.Generic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private readonly Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType;
        private readonly Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType;

        private readonly DefaultServerPacketProcessor defaultPacketProcessor;

        public PacketHandler(TcpServer tcpServer, TimeKeeper timeKeeper, SimulationOwnership simulationOwnership)
        {
            defaultPacketProcessor = new DefaultServerPacketProcessor(tcpServer);

            Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>
            {
                {typeof(TcpServer), tcpServer },
                {typeof(TimeKeeper), timeKeeper },
                {typeof(SimulationOwnership), simulationOwnership },
                {typeof(EscapePodManager), new EscapePodManager() }
            };

            authenticatedPacketProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(AuthenticatedPacketProcessor<>));

            unauthenticatedPacketProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(UnauthenticatedPacketProcessor<>));
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
                Log.Info("Received invalid, unauthenticated packet: " + packet);
            }
        }
    }
}
