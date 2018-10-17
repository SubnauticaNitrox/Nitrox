using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public ulong LPlayerId { get; }
        public string Text { get; }

        public ChatMessage(ulong playerId, string text)
        {
            LPlayerId = playerId;
            Text = text;
        }
    }
}
