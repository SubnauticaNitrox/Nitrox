using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class VehicleDocking : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxId DockId { get; }
        public ushort PlayerId { get; }

        public VehicleDocking(NitroxId vehicleId, NitroxId dockId, ushort playerId)
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
