using System;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Networking;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return $"[VehicleMovement - {base.ToString()}, Data: {VehicleMovementData}]";
        }
    }
}
