using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public NitroxId PlayerId { get; }
        public string Text { get; }
        public static readonly NitroxId SERVER_ID = NitroxId.Empty;

        public ChatMessage(NitroxId playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
