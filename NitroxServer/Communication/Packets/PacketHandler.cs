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
        private readonly Dictionary<PacketProcessorCacheKey, PacketProcessor> packetProcessorCache = new();

        public PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor)
        {
            this.playerManager = playerManager;
            defaultServerPacketProcessor = packetProcessor;
        }

        public void Process(Packet packet, NitroxConnection connection)
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
            PacketProcessorCacheKey cacheKey = new(packet.GetType(), true);
            if (!packetProcessorCache.TryGetValue(cacheKey, out PacketProcessor processor))
            {
                Type packetProcessorType = typeof(AuthenticatedPacketProcessor<>).MakeGenericType(cacheKey.PacketType);
                packetProcessorCache[cacheKey] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
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

        private void ProcessUnauthenticated(Packet packet, NitroxConnection connection)
        {
            PacketProcessorCacheKey cacheKey = new(packet.GetType(), false);
            if (!packetProcessorCache.TryGetValue(cacheKey, out PacketProcessor processor))
            {
                Type packetProcessorType = typeof(UnauthenticatedPacketProcessor<>).MakeGenericType(cacheKey.PacketType);
                packetProcessorCache[cacheKey] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
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

        private readonly record struct PacketProcessorCacheKey(Type PacketType, bool IsAuthenticatedLookup);
    }
}
