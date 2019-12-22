using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleDocking : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxId DockId { get; }
        public NitroxId PlayerId { get; }

        public VehicleDocking(NitroxId vehicleId, NitroxId dockId, NitroxId playerId)
        {
            VehicleId = vehicleId;
            DockId = dockId;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[VehicleDocking VehicleId: " + VehicleId + " DockId: " + DockId + " PlayerId: " + PlayerId + "]";
        }
    }
}
