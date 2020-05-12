using System;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxModel.Packets
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
