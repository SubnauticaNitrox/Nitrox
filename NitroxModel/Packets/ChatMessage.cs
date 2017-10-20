using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public string PlayerId { get; }
        public string Text { get; }
        public Color Color { get; }

        public ChatMessage(string playerId, string text, Color color)
        {
            PlayerId = playerId;
            Text = text;
            Color = color;
        }
    }
}
