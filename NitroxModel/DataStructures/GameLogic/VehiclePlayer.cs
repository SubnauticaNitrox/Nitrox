using ProtoBuf;
using System;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class VehiclePlayer
    {
        [ProtoMember(1)]
        public string CurrentPlayer { get; }

        public VehiclePlayer()
        {
            // For serialization purposes
        }

        public VehiclePlayer(string currentPlayer)
        {
            CurrentPlayer = currentPlayer;
        }
    }
}
