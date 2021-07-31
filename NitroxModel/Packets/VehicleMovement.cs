using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public VehicleMovementData VehicleMovementData { get; }

        public VehicleMovement(ushort playerId, VehicleMovementData vehicleMovementData) : base(playerId, vehicleMovementData.Position, vehicleMovementData.Velocity, vehicleMovementData.Rotation, vehicleMovementData.Rotation)
        {
            VehicleMovementData = vehicleMovementData;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }
    }
}
