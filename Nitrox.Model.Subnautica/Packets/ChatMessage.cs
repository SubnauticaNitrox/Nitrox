﻿using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class ChatMessage : Packet
    {
        public ushort PlayerId { get; }
        public string Text { get; }
        public const ushort SERVER_ID = ushort.MaxValue;

        public ChatMessage(ushort playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
