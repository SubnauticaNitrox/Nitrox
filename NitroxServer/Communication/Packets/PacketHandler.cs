using System;
using System.Collections.Generic;
using NitroxModel.Core;
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
        private readonly Dictionary<Type, PacketProcessor> packetProcessorAuthCache = new();
        private readonly Dictionary<Type, PacketProcessor> packetProcessorUnauthCache = new();

        public PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor)
        {
            this.playerManager = playerManager;
            defaultServerPacketProcessor = packetProcessor;
        }

        public void Process(Packet packet, INitroxConnection connection)
        {
            Player player = playerManager.GetPlayer(connection);
            if (player == null)
            {
                ProcessUnauthenticated(packet, connection);
            }
            else
            {
                ProcessAuthenticated(packet, player);
            }
        }

        private void ProcessAuthenticated(Packet packet, Player player)
        {
            Type packetType = packet.GetType();
            if (!packetProcessorAuthCache.TryGetValue(packetType, out PacketProcessor processor))
            {
                Type packetProcessorType = typeof(AuthenticatedPacketProcessor<>).MakeGenericType(packetType);
                packetProcessorAuthCache[packetType] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
            }

            if (processor != null)
            {
                try
                {
                    processor.ProcessPacket(packet, player);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error in packet processor {processor.GetType()}");
                }
            }
            else
            {
                defaultServerPacketProcessor.ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, INitroxConnection connection)
        {
            Type packetType = packet.GetType();
            if (!packetProcessorUnauthCache.TryGetValue(packetType, out PacketProcessor processor))
            {
                Type packetProcessorType = typeof(UnauthenticatedPacketProcessor<>).MakeGenericType(packetType);
                packetProcessorUnauthCache[packetType] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
            }
            if (processor == null)
            {
                Log.Warn($"Received invalid, unauthenticated packet: {packet}");
                return;
            }

            try
            {
                processor.ProcessPacket(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in packet processor {processor.GetType()}");
            }
        }
    }
}
