﻿using NitroxClient.Communication;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Chat
    {
        private readonly PacketSender packetSender;

        public Chat(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SendChatMessage(string text)
        {
            ChatMessage message = new ChatMessage(packetSender.PlayerId, text);
            packetSender.Send(message);
        }
    }
}
