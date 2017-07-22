using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class Chat
    {
        private PacketSender packetSender;

        public Chat(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SendChatMessage(String text)
        {
            ChatMessage message = new ChatMessage(packetSender.PlayerId, text);
            packetSender.Send(message);
        }

    }
}
