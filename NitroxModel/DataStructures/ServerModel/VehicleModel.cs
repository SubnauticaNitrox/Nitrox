using UnityEngine;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public TechType TechType { get; }
        public string Guid { get; set; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Velocity { get; }
        public Vector3 AngularVelocity { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }
        public bool AppliedThrottle { get; }

        public VehicleModel(TechType techType, string guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle)
        {
            TechType = techType;
            Guid = guid;

            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;

            SteeringWheelYaw = steeringWheelYaw;
            SteeringWheelPitch = steeringWheelPitch;
            AppliedThrottle = appliedThrottle;
        }
    }
}
