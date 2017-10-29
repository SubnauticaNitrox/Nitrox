using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : AuthenticatedPacket
    {
        public string Text { get; }

        public ChatMessage(string playerId, string text) : base(playerId)
        {
            Text = text;
        }
    }
}
