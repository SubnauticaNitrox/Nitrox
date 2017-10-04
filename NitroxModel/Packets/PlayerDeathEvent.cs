using UnityEngine;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : AuthenticatedPacket
    {
        public Vector3 DeathPosition { get; private set; }

        public PlayerDeathEvent(String playerId, Vector3 deathPosition) : base(playerId)
        {
            this.DeathPosition = deathPosition;
        }
    }
}
