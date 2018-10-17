using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public ulong LPlayerId { get; }
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(ulong playerId, Vector3 deathPosition)
        {
            LPlayerId = playerId;
            DeathPosition = deathPosition;
        }
    }
}
