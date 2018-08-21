using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public ushort PlayerId { get; }
        public string Text { get; }

        public ChatMessage(ushort playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
