using System;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<Packet>
    {
        private readonly PlayerManager playerManager;

        private readonly HashSet<Type> loggingPacketBlackList = new()
        {
            typeof(AnimationChangeEvent),
            typeof(Movement),
            typeof(VehicleMovement),
            typeof(ItemPosition),
            typeof(PlayerStats),
            typeof(VehicleColorChange),
            typeof(StoryEventSend),
            typeof(PlayFMODAsset),
            typeof(PlayFMODCustomEmitter),
            typeof(PlayFMODCustomLoopingEmitter),
            typeof(PlayFMODStudioEmitter),
            typeof(PlayerCinematicControllerCall)
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

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
