using System;
using UnityEngine;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public TechType TechType { get; }
        public String Guid { get; set; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Velocity { get; }
        public Vector3 AngularVelocity { get; }
        public float SteeringWheelYaw { get; }
        public float SteeringWheelPitch { get; }
        public bool AppliedThrottle { get; }

        public VehicleModel(TechType techType, String guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle)
        {
            this.TechType = techType;
            this.Guid = guid;

            this.Position = position;
            this.Rotation = rotation;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;

            this.SteeringWheelYaw = steeringWheelYaw;
            this.SteeringWheelPitch = steeringWheelPitch;
            this.AppliedThrottle = appliedThrottle;
        }
    }
}
