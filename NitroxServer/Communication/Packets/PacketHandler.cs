using System;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxServer.Communication.NetworkingLayer;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private readonly PlayerManager playerManager;
        private readonly DefaultServerPacketProcessor defaultServerPacketProcessor;

        public PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor)
        {
            this.playerManager = playerManager;
            defaultServerPacketProcessor = packetProcessor;
        }

        public void Process(Packet packet, INitroxConnection connection)
        {
            Player player = playerManager.GetPlayer(connection);
            Packet unpackedPacket = packet;
            if (packet.GetType() == typeof(TunneledPacket))
            {
                unpackedPacket = ((TunneledPacket)packet).Packet;
            }

            if (player == null)
            {
                ProcessUnauthenticated(unpackedPacket, connection);
            }
            else
            {
                ProcessAuthenticated(unpackedPacket, player);
            }
        }

        private void ProcessAuthenticated(Packet packet, Player player)
        {
            Type serverPacketProcessorType = typeof(AuthenticatedPacketProcessor<>);
            Type packetType = packet.GetType();
            Type packetProcessorType = serverPacketProcessorType.MakeGenericType(packetType);

            Optional<object> opProcessor = NitroxServiceLocator.LocateOptionalService(packetProcessorType);

            if (opProcessor.HasValue)
            {
                PacketProcessor processor = (PacketProcessor)opProcessor.Value;
                processor.ProcessPacket(packet, player);
            }
            else
            {
                defaultServerPacketProcessor.ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, INitroxConnection connection)
        {
            try
            {
                Type serverPacketProcessorType = typeof(UnauthenticatedPacketProcessor<>);
                Type packetType = packet.GetType();
                Type packetProcessorType = serverPacketProcessorType.MakeGenericType(packetType);

                PacketProcessor processor = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);
                processor.ProcessPacket(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Received invalid, unauthenticated packet: {packet}");
            }
        }
    }
}
