using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : AuthenticatedPacket
    {
        public String Text { get; set; }

        public ChatMessage(String playerId, String text) : base(playerId)
        {
            this.Text = text;
        }
    }
}
