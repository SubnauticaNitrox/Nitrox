using System;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<Packet>
{
    private readonly PlayerManager playerManager;

    private readonly HashSet<Type> loggingPacketBlackList = new()
    {
        typeof(AnimationChangeEvent),
        typeof(PlayerMovement),
        typeof(VehicleMovement),
        typeof(ItemPosition),
        typeof(PlayerStats),
        typeof(StoryGoalExecuted),
        typeof(FMODAssetPacket),
        typeof(FMODCustomEmitterPacket),
        typeof(FMODCustomLoopingEmitterPacket),
        typeof(FMODStudioEmitterPacket),
        typeof(PlayerCinematicControllerCall)
    };

    /// <summary>
    /// Packet types which don't have a server packet processor but should not be transmitted
    /// </summary>
    private readonly HashSet<Type> defaultPacketProcessorBlacklist = new()
    {
        typeof(GameModeChanged)
    };

    public DefaultServerPacketProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(Packet packet, Player player)
    {
        if (!loggingPacketBlackList.Contains(packet.GetType()))
        {
            Log.Debug($"Using default packet processor for: {packet} and player {player.Id}");
        }

        if (defaultPacketProcessorBlacklist.Contains(packet.GetType()))
        {
            Log.ErrorOnce($"Player {player.Name} [{player.Id}] sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.");
            return;
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
