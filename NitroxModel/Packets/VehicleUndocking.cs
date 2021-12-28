using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleUndocking : Packet
    {
        [Index(0)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(1)]
        public virtual NitroxId DockId { get; protected set; }
        [Index(2)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(3)]
        public virtual bool UndockingStart { get; protected set; }

        public VehicleUndocking() { }

        public VehicleUndocking(NitroxId vehicleId, NitroxId dockId, ushort playerId, bool undockingStart)
        {
            VehicleId = vehicleId;
            DockId = dockId;
            PlayerId = playerId;
            UndockingStart = undockingStart;
        }
    }
}
