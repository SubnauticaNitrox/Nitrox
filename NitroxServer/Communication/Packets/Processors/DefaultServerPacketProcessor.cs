using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<AuthenticatedPacket>
    {
        private readonly TcpServer tcpServer;

        private readonly HashSet<Type> loggingPacketBlackList = new HashSet<Type> {
            typeof(AnimationChangeEvent),
            typeof(Movement),
            typeof(VehicleMovement),
            typeof(ItemPosition),
            typeof(CyclopsChangeColor)
        };

        public DefaultServerPacketProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void Process(AuthenticatedPacket packet, Player player)
        {
            if (!loggingPacketBlackList.Contains(packet.GetType()))
            {
                Log.Debug("Using default packet processor for: " + packet.ToString() + " and player " + player.Id);
            }

            tcpServer.SendPacketToOtherPlayers(packet, player);
        }
    }
}
