using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class DefaultServerPacketProcessor(IPacketSender packetSender, ILogger<DefaultServerPacketProcessor> logger) : AuthenticatedPacketProcessor<Packet>
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

    private readonly IPacketSender packetSender = packetSender;

    public override void Process(Packet packet, Player player)
    {
        Type packetType = packet.GetType();
        if (!loggingPacketBlackList.Contains(packetType))
        {
            logger.ZLogDebug($"Using default packet processor for: {packet} and player {player.Id}");
        }
        if (defaultPacketProcessorBlacklist.Contains(packetType))
        {
            logger.ZLogErrorOnce($"Player {player.Name} [{player.Id}] sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.");
            return;
        }

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
