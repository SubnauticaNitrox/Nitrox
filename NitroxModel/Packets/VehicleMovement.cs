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
        public Vector3 AngularVelocity { get { return serializableAngularVelocity.ToVector3(); } }
        public String Guid { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }
        public bool AppliedThrottle { get; }

        private SerializableTechType serializableTechType;
        private SerializableVector3 serializableAngularVelocity;

        public VehicleMovement(String playerId, Vector3 playerPosition, Vector3 velocity, Quaternion rotation, Vector3 angularVelocity, TechType techType, String guid, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle) : base(playerId, playerPosition, velocity, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.serializableTechType = new SerializableTechType(techType);
            this.serializableAngularVelocity = SerializableVector3.from(angularVelocity);

            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;

            this.AppliedThrottle = appliedThrottle;

            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}
