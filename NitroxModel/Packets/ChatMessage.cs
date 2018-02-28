using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public string PlayerId { get; }
        public string Text { get; }

        public ChatMessage(string playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
