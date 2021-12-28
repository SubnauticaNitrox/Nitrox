using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleCreated : Packet
    {
        [Index(0)]
        public virtual string PlayerName { get; protected set; }
        [Index(1)]
        public virtual VehicleModel CreatedVehicle { get; protected set; }

        public VehicleCreated() { }

        public VehicleCreated(VehicleModel createdVehicle, string playerName)
        {
            CreatedVehicle = createdVehicle;
            PlayerName = playerName;
        }
    }
}
