using System;
using UnityEngine;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public TechType TechType { get { return serializableTechType.TechType; } }
        public String Guid { get; set; }
        public Vector3 Position { get { return serializablePosition.ToVector3(); } }
        public Quaternion Rotation { get { return serializableRotation.ToQuaternion(); } }
        public Vector3 Velocity { get { return serializableVelocity.ToVector3(); } }
        public Vector3 AngularVelocity { get { return serializableAngularVelocity.ToVector3(); } }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }
        public bool AppliedThrottle { get; }

        private SerializableTechType serializableTechType;

        private SerializableVector3 serializablePosition;
        private SerializableQuaternion serializableRotation;
        private SerializableVector3 serializableVelocity;
        private SerializableVector3 serializableAngularVelocity;

        public VehicleModel(TechType techType, String guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle)
        {
            this.serializableTechType = new SerializableTechType(techType);
            this.Guid = guid;

            this.serializablePosition = SerializableVector3.from(position);
            this.serializableRotation = SerializableQuaternion.from(rotation);
            this.serializableVelocity = SerializableVector3.from(velocity);
            this.serializableAngularVelocity = SerializableVector3.from(angularVelocity);

            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;
            this.AppliedThrottle = appliedThrottle;
        }
    }
}
