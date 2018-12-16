using System;
using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialRemotePlayerData
    {
        public PlayerContext PlayerContext { get; set; }
        
        public Vector3 Position { get; set; }
        
        public InitialRemotePlayerData()
        {
            // Constructor for serialization
        }

        public InitialRemotePlayerData(PlayerContext playerContext, Vector3 position)
        {
            PlayerContext = playerContext;
            Position = position;
        }
    }
}
