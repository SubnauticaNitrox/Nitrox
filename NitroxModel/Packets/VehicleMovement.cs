using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public TechType TechType { get { return serializableTechType.TechType; } }
        public String Guid { get; }

        private SerializableTechType serializableTechType;

        public VehicleMovement(String playerId, Vector3 playerPosition, Vector3 velocity, Quaternion rotation, TechType techType, String guid) : base(playerId, playerPosition, velocity, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.serializableTechType = new SerializableTechType(techType);
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}
