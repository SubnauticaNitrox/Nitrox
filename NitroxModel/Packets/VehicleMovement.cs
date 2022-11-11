using System;
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
        public override NitroxVector3 Position => VehicleMovementData.Position;
        public override NitroxVector3 Velocity => VehicleMovementData.Velocity;
        public override NitroxQuaternion BodyRotation => VehicleMovementData.Rotation;
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
