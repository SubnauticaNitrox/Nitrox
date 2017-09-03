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
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }

        private SerializableTechType serializableTechType;

        public VehicleMovement(String playerId, Vector3 playerPosition, Vector3 velocity, Quaternion rotation, TechType techType, String guid, float steeringWheelYaw, float steeringWheelPitch) : base(playerId, playerPosition, velocity, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.serializableTechType = new SerializableTechType(techType);

            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;

            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}
