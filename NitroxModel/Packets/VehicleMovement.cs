using System;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public override ushort PlayerId { get; }
        public VehicleMovementData VehicleMovementData { get; }

        [IgnoredMember]
        public override NitroxVector3 Position => VehicleMovementData.Position;
        [IgnoredMember]
        public override NitroxVector3 Velocity => VehicleMovementData.Velocity;
        [IgnoredMember]
        public override NitroxQuaternion BodyRotation => VehicleMovementData.Rotation;
        [IgnoredMember]
        public override NitroxQuaternion AimingRotation => VehicleMovementData.Rotation;

        public VehicleMovement(ushort playerId, VehicleMovementData vehicleMovementData)
        {
            PlayerId = playerId;
            VehicleMovementData = vehicleMovementData;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }
    }
}
