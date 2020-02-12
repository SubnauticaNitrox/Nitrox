using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Logger;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ChatMessageProcessor : AuthenticatedPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager playerManager;

        public ChatMessageProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(ChatMessage packet, Player player)
        {
            Log2.Instance.Log(NLogType.Info, string.Format("{0} said: {1}", player.Name, packet.Text));

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
