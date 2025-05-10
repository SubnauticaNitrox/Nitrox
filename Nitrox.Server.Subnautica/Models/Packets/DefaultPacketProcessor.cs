using System;
using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets;

/// <summary>
///     The default packet processor for packets which don't define one. This processor will send those packets to other
///     players as they were received.
/// </summary>
internal sealed class DefaultPacketProcessor(PlayerRepository playerRepository, ILogger<DefaultPacketProcessor> logger) : IAuthPacketProcessor<Packet>
{
    /// <summary>
    ///     Packet types which don't have a server packet processor but should not be transmitted
    /// </summary>
    private readonly HashSet<Type> defaultPacketProcessorBlacklist =
    [
        typeof(GameModeChanged),
        typeof(DropSimulationOwnership)
    ];

    private readonly ILogger<DefaultPacketProcessor> logger = logger;

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
        typeof(SeaTreaderChunkPickedUp)
    ];

    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task Process(AuthProcessorContext context, Packet packet)
    {
        if (!loggingPacketBlackList.Contains(packet.GetType()))
        {
            logger.ZLogDebug($"Using default packet processor for: {packet:@Packet} and session #{context.Sender.SessionId:@SessionId}");
        }

        if (defaultPacketProcessorBlacklist.Contains(packet.GetType()))
        {
            ConnectedPlayerDto player = await playerRepository.GetConnectedPlayerBySessionIdAsync(context.Sender.SessionId);
            if (player != null)
            {
                // TODO: Log error once
                logger.ZLogError($"Player {player.Name:@PlayerName} [{player.Id:@PlayerId}] sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.");
            }
        }
        await context.ReplyToOthers(packet);
    }
}
