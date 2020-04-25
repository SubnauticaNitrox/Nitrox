using System;
using NitroxModel.Packets.Core;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet, IVolatilePacket
    {
        public string PlayerName { get; }
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(string playerName, Vector3 deathPosition)
        {
            PlayerName = playerName;
            DeathPosition = deathPosition;
        }
    }
}
