using System;
using UnityEngine;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public PlayerContext PlayerContext;
        public Vector3 DeathPosition { get; }

        public PlayerDeathEvent(PlayerContext playerContext, Vector3 deathPosition)
        {
            PlayerContext = playerContext;
            DeathPosition = deathPosition;
        }
    }
}
