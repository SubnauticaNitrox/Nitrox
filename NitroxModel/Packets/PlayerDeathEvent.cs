using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
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
