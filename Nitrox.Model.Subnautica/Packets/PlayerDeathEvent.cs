using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
