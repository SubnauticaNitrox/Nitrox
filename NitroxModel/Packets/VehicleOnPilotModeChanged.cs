using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public string VehicleGuid { get; }
        public ushort PlayerId { get; }
        public bool IsPiloting { get; }

        public VehicleOnPilotModeChanged(string vehicleGuid, ushort playerId, bool isPiloting)
        {
            VehicleGuid = vehicleGuid;
            PlayerId = playerId;
            IsPiloting = isPiloting;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotModeChanged - VehicleGuid: " + VehicleGuid + " PlayerId: " + PlayerId + " IsPiloting: " + IsPiloting + "]";
        }
    }
}
