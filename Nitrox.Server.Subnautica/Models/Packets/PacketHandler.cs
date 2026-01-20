using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets;

internal sealed class PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor, ILogger<PacketHandler> logger)
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly DefaultServerPacketProcessor defaultServerPacketProcessor = packetProcessor;
    private readonly ILogger<PacketHandler> logger = logger;
    private readonly Dictionary<Type, PacketProcessor> packetProcessorAuthCache = new();
    private readonly Dictionary<Type, PacketProcessor> packetProcessorUnauthCache = new();

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
                logger.ZLogError(ex, $"Error in packet processor {processor.GetType()}");
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
            logger.ZLogWarning($"Received invalid, unauthenticated packet: {packet}");
            return;
        }

        try
        {
            processor.ProcessPacket(packet, connection);
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error in packet processor {processor.GetType()}");
        }
    }
}
