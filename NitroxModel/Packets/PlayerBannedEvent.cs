using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerBannedEvent : Packet
    {
        public string PlayerName { get; }

        public PlayerBannedEvent(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
