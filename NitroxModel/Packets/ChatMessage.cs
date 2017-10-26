using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : AuthenticatedPacket
    {
        public String Text { get; }

        public ChatMessage(String playerId, String text) : base(playerId)
        {
            Text = text;
        }
    }
}
