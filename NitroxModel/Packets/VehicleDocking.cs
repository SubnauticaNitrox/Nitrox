using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleDocking : Packet
    {
        public string VehicleGuid { get; }
        public string DockGuid { get; }
        public ushort PlayerId { get; }

        public VehicleDocking(string vehicleGuid, string dockGuid, ushort playerId)
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
