using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public ulong PlayerId { get; }
        public string Text { get; }

        public ChatMessage(ulong playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
