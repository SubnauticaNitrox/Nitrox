using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialRemotePlayerData
    {
        public PlayerContext PlayerContext { get; set; }        
        public Vector3 Position { get; set; }
        public Optional<string> SubRootGuid { get; }
        public List<TechType> EquippedTechTypes { get; }

        public InitialRemotePlayerData()
        {
            // Constructor for serialization
        }

        public InitialRemotePlayerData(PlayerContext playerContext, Vector3 position, Optional<string> subRootGuid, List<TechType> equippedTechTypes)
        {
            PlayerContext = playerContext;
            Position = position;
            SubRootGuid = subRootGuid;
            EquippedTechTypes = equippedTechTypes;
        }
    }
}
