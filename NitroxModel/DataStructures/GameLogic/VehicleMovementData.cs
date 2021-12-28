using NitroxModel.DataStructures.Unity;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class VehicleMovementData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxTechType TechType { get; protected set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxId Id { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual NitroxVector3 Position { get; protected set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual NitroxQuaternion Rotation { get; protected set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual NitroxVector3 Velocity { get; protected set; }

        [Index(5)]
        [ProtoMember(6)]
        public virtual NitroxVector3 AngularVelocity { get; protected set; }

        [Index(6)]
        [ProtoMember(7)]
        public virtual float SteeringWheelYaw { get; protected set; }

        [Index(7)]
        [ProtoMember(8)]
        public virtual float SteeringWheelPitch { get; protected set; }

        [Index(8)]
        [ProtoMember(9)]
        public virtual bool AppliedThrottle { get; protected set; }

        public VehicleMovementData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public VehicleMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 velocity, NitroxVector3 angularVelocity, float steeringWheelYaw, float steeringWheelPitch, bool appliedThrottle)
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
