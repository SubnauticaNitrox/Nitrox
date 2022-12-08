using System;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

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

        public void Process(Packet packet, NitroxConnection connection)
        {
            Player player = playerManager.GetPlayer(connection);
            if (player != null)
            {
                ProcessAuthenticated(packet, player);
            }
            else if (!playerManager.GetQueuedPlayers().Contains(connection))
            {
                ProcessUnauthenticated(packet, connection);
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

        private void ProcessUnauthenticated(Packet packet, NitroxConnection connection)
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
