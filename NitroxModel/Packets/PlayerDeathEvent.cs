using UnityEngine;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public string PlayerId { get; }
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(string playerId, Vector3 deathPosition)
        {
            PlayerId = playerId;
            DeathPosition = deathPosition;
        }
    }
}
