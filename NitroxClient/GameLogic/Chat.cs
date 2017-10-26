using NitroxClient.Communication;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Chat
    {
        private readonly IPacketSender packetSender;

        public Chat(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SendChatMessage(string text, Color color)
        {
            ChatMessage message = new ChatMessage(packetSender.PlayerId, text, color);
            packetSender.Send(message);
        }

        public void SendPlayerJoin(string text, Color color)
        {
            PlayerJoin packet = new PlayerJoin(packetSender.PlayerId, color);
            packetSender.Send(packet);
        }
    }
}
