using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleDocking : Packet
    {
        [Index(0)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(1)]
        public virtual NitroxId DockId { get; protected set; }
        [Index(2)]
        public virtual ushort PlayerId { get; protected set; }

        public VehicleDocking() { }

        public VehicleDocking(NitroxId vehicleId, NitroxId dockId, ushort playerId)
        {
            VehicleId = vehicleId;
            DockId = dockId;
            PlayerId = playerId;
        }
    }
}
