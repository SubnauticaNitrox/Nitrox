using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleDocking : Packet
    {
        public string VehicleGuid { get; }
        public string DockGuid { get; }
        public ulong PlayerId { get; }

        public VehicleDocking(string vehicleGuid, string dockGuid, ulong playerId)
        {
            VehicleGuid = vehicleGuid;
            DockGuid = dockGuid;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return "[VehicleDocking VehicleGuid: " + VehicleGuid + " DockGuid: " + DockGuid + " PlayerId: " + PlayerId + "]";
        }
    }
}
