using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public abstract class VehicleMovementData
    {
        [DataMember(Order = 1)]
        public NitroxTechType TechType { get; }

        [DataMember(Order = 2)]
        public NitroxId Id { get; set; }

        [DataMember(Order = 3)]
        public NitroxVector3 Position { get; }

        [DataMember(Order = 4)]
        public NitroxQuaternion Rotation { get; }

        [DataMember(Order = 5)]
        public NitroxVector3 Velocity { get; }

        [DataMember(Order = 6)]
        public NitroxVector3 AngularVelocity { get; }

        [DataMember(Order = 7)]
        public float SteeringWheelYaw { get; }

        [DataMember(Order = 8)]
        public float SteeringWheelPitch { get; }

        [DataMember(Order = 9)]
        public bool AppliedThrottle { get; }

        [DataMember(Order = 10)]
        public NitroxVector3? DriverPosition { get; set; }

        [IgnoreConstructor]
        protected VehicleMovementData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, NitroxVector3? driverPosition = null)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            SteeringWheelYaw = steeringWheelYaw;
            SteeringWheelPitch = steeringWheelPitch;
            AppliedThrottle = appliedThrottle;
            DriverPosition = driverPosition;
        }

        public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
            Velocity = NitroxVector3.Zero;
            AngularVelocity = NitroxVector3.Zero;
            SteeringWheelYaw = 0f;
            SteeringWheelPitch = 0f;
            AppliedThrottle = false;
        }

        public override string ToString()
        {
            return $"[VehicleMovementData - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Velocity: {Velocity}, AngularVelocity: {AngularVelocity}, SteeringWheelYaw: {SteeringWheelYaw}, SteeringWheelPitch: {SteeringWheelPitch}, AppliedThrottle: {AppliedThrottle}]";
        }
    }
}
