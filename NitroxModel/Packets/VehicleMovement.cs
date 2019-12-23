using System;
using NitroxModel.DataStructures.GameLogic;
using Lidgren.Network;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public VehicleMovementData Vehicle { get; }

        public VehicleMovement(ushort playerId, VehicleMovementData vehicle) : base(playerId, vehicle.Position, vehicle.Velocity, vehicle.Rotation, vehicle.Rotation)
        {
            Vehicle = vehicle;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UnreliableSequenced;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }

        public override string ToString()
        {
            return "[VehicleMovement - vehicle: " + Vehicle +
                "]\n\t" + base.ToString();
        }
    }
}
