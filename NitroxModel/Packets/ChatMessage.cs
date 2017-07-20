using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : PlayerPacket
    {
        public String Text { get; set; }

        public ChatMessage(String playerId, String text) : base(playerId)
        {
            this.Text = text;
        }
    }
}
