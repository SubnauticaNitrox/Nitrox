using System;
using LiteNetLib;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public VehicleMovementData Vehicle { get; }

        public VehicleMovement(ushort playerId, VehicleMovementData vehicle) : base(playerId, vehicle.Position, vehicle.Velocity, vehicle.Rotation, vehicle.Rotation)
        {
            Vehicle = vehicle;
            DeliveryMethod = DeliveryMethod.Sequenced;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }

        public override string ToString()
        {
            return "[VehicleMovement - vehicle: " + Vehicle +
                "]\n\t" + base.ToString();
        }
    }
}
