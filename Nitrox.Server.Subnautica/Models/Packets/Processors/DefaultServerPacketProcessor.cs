using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class DefaultServerPacketProcessor(ILogger<DefaultServerPacketProcessor> logger) : IAuthPacketProcessor<Packet>
{
    /// <summary>
    ///     Packet types which don't have a server packet processor but should not be transmitted
    /// </summary>
    private readonly HashSet<Type> defaultPacketProcessorBlacklist =
    [
        typeof(GameModeChanged),
        typeof(DropSimulationOwnership)
    ];

    private readonly ILogger<DefaultServerPacketProcessor> logger = logger;

    private readonly HashSet<Type> loggingPacketBlackList =
    [
        typeof(AnimationChangeEvent),
        typeof(PlayerMovement),
        typeof(ItemPosition),
        typeof(PlayerStats),
        typeof(StoryGoalExecuted),
        typeof(FMODAssetPacket),
        typeof(FMODCustomEmitterPacket),
        typeof(FMODCustomLoopingEmitterPacket),
        typeof(FMODStudioEmitterPacket),
        typeof(PlayerCinematicControllerCall),
        typeof(TorpedoShot),
        typeof(TorpedoHit),
        typeof(TorpedoTargetAcquired),
        typeof(StasisSphereShot),
        typeof(StasisSphereHit),
        typeof(SeaTreaderChunkPickedUp),
        typeof(ToggleLights)
    ];

    public async Task Process(AuthProcessorContext context, Packet packet)
    {
        Type packetType = packet.GetType();
        if (!loggingPacketBlackList.Contains(packetType))
        {
            logger.ZLogDebug($"Using default packet processor for: {packet} and player #{context.Sender.SessionId}");
        }
        if (defaultPacketProcessorBlacklist.Contains(packetType))
        {
            logger.ZLogErrorOnce($"Player {context.Sender.Name} #{context.Sender.SessionId} sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.");
            return;
        }

        await context.SendToOthersAsync(packet);
    }
}
