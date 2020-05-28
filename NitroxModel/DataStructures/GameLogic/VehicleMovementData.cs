using ProtoBufNet;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class VehicleMovementData
    {
        [ProtoMember(1)]
        public NitroxTechType TechType { get; }

        [ProtoMember(2)]
        public NitroxId Id { get; set; }

        [ProtoMember(3)]
        public NitroxVector3 Position { get; }

        [ProtoMember(4)]
        public NitroxQuaternion Rotation { get; }

        [ProtoMember(5)]
        public NitroxVector3 Velocity { get; }

        [ProtoMember(6)]
        public NitroxVector3 AngularVelocity { get; }

        [ProtoMember(7)]
        public float SteeringWheelYaw { get; }

        [ProtoMember(8)]
        public float SteeringWheelPitch { get; }

        [ProtoMember(9)]
        public bool AppliedThrottle { get; }

        [ProtoMember(10)]
        public float Health { get; }

        protected VehicleMovementData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle, float health)
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
            Health = health;
        }

        public override string ToString()
        {
            return $"[TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Velocity: {Velocity}, AngularVelocity: {AngularVelocity}, SteeringWheelYaw: {SteeringWheelYaw}, SteeringWheelPitch: {SteeringWheelPitch}, AppliedThrottle: {AppliedThrottle}, Health: {Health}]";
        }
    }
}
