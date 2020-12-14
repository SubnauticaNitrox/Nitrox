using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public VehicleMovementData VehicleMovementData { get; }

        public VehicleMovement(ushort playerId, VehicleMovementData vehicleMovementData) : base(playerId, vehicleMovementData.Position, vehicleMovementData.Velocity, vehicleMovementData.Rotation, vehicleMovementData.Rotation)
        {
            VehicleMovementData = vehicleMovementData;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }

        public override string ToString()
        {
            return $"[VehicleMovement - VehicleMovementData: {VehicleMovementData}, Movement: {base.ToString()}]";
        }
    }
}
