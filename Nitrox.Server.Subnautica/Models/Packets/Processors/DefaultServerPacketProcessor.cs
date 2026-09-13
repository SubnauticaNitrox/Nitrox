using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class DefaultServerPacketProcessor(PlayerService playerService, ILogger<DefaultServerPacketProcessor> logger) : IAuthPacketProcessor<Packet>
{
    /// <summary>
    ///     Packet types which don't have a server packet processor but should not be transmitted
    /// </summary>
    private readonly HashSet<Type> defaultPacketProcessorBlacklist =
    [
        typeof(GameModeChanged),
        typeof(DropSimulationOwnership)
    ];

    private readonly PlayerService playerService = playerService;
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
            logger.ZLogDebug($"Transmitting data from player {GetPlayerInfo(context)}: {packet}");
        }
        if (defaultPacketProcessorBlacklist.Contains(packetType))
        {
            logger.ZLogErrorOnce($"Player {GetPlayerInfo(context)} sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.");
            return;
        }

        await context.SendToOthersAsync(packet);
    }

    private string GetPlayerInfo(AuthProcessorContext context)
    {
        return $"'{playerService.GetProperty<NameProperty>(context.Sender).Value}' #{context.Sender}";
    }
}
