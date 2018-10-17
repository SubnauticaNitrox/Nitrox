using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<Packet>
    {
        private readonly PlayerManager playerManager;

        private readonly HashSet<Type> loggingPacketBlackList = new HashSet<Type> {
            typeof(AnimationChangeEvent),
            typeof(Movement),
            typeof(VehicleMovement),
            typeof(ItemPosition),
            typeof(PlayerStats),
            typeof(PowerLevelChanged),
            typeof(VehicleColorChange),
            typeof(StoryEventSend)
        };

        public DefaultServerPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(Packet packet, Player player)
        {
            if (!loggingPacketBlackList.Contains(packet.GetType()))
            {
                Log.Debug("Using default packet processor for: " + packet.ToString() + " and player " + player.LId);
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
