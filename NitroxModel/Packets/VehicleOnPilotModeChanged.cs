using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleOnPilotModeChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(1)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(2)]
        public virtual bool IsPiloting { get; protected set; }

        private VehicleOnPilotModeChanged() { }

        public VehicleOnPilotModeChanged(NitroxId vehicleId, ushort playerId, bool isPiloting)
        {
            VehicleId = vehicleId;
            PlayerId = playerId;
            IsPiloting = isPiloting;
        }
    }
}
