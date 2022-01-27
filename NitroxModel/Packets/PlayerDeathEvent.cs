using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public ushort PlayerId { get; }
        public NitroxVector3 DeathPosition { get; }

        public PlayerDeathEvent(ushort playerId, NitroxVector3 deathPosition)
        {
            PlayerId = playerId;
            DeathPosition = deathPosition;
        }
    }
}
