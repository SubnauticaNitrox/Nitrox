using NitroxModel.DataStructures.Util;
using ProtoBuf;
using System;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class VehicleMovementData
    {
        [ProtoMember(1)]
        public TechType TechType { get; }

        [ProtoMember(2)]
        public string Guid { get; set; }

        [ProtoMember(3)]
        public Vector3 Position { get; }

        [ProtoMember(4)]
        public Quaternion Rotation { get; }

        [ProtoMember(5)]
        public Vector3 Velocity { get; }

        [ProtoMember(6)]
        public Vector3 AngularVelocity { get; }

        [ProtoMember(7)]
        public float SteeringWheelYaw { get; }

        [ProtoMember(8)]
        public float SteeringWheelPitch { get; }

        [ProtoMember(9)]
        public bool AppliedThrottle { get; }

        [ProtoMember(10)]
        public Vector3 LeftAimTarget { get; }

        [ProtoMember(11)]
        public Vector3 RightAimTarget { get; }


        public VehicleMovementData()
        {
            // For serialization purposes
        }

        public VehicleMovementData(TechType techType, string guid, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle,
            Vector3 leftAimTarget, Vector3 rightAimTarget)
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
            LeftAimTarget = leftAimTarget;
            RightAimTarget = rightAimTarget;
        }
    }
}
