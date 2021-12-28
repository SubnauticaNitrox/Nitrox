using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleMovement : Movement
    {
        [Index(5)]
        public virtual VehicleMovementData VehicleMovementData { get; protected set; }

        public VehicleMovement() { }

        public VehicleMovement(ushort playerId, VehicleMovementData vehicleMovementData) : base(playerId, vehicleMovementData.Position, vehicleMovementData.Velocity, vehicleMovementData.Rotation, vehicleMovementData.Rotation)
        {
            VehicleMovementData = vehicleMovementData;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }
    }
}
