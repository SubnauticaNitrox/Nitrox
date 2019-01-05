using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public ushort PlayerId { get; }
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(ushort playerId, Vector3 deathPosition)
        {
            PlayerId = playerId;
            DeathPosition = deathPosition;
        }
    }
}
