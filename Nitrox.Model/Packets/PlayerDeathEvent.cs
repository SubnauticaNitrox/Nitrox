using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public string PlayerName { get; }
        public NitroxVector3 DeathPosition { get; }

        public PlayerDeathEvent(string playerName, NitroxVector3 deathPosition)
        {
            PlayerName = playerName;
            DeathPosition = deathPosition;
        }
    }
}
