using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public string VehicleGuid { get; }
        public string PlayerGuid { get; }
        public bool IsPiloting { get; }

        public VehicleOnPilotModeChanged(string vehicleGuid, string playerGuid, bool isPiloting)
        {
            VehicleGuid = vehicleGuid;
            PlayerGuid = playerGuid;
            IsPiloting = isPiloting;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotModeChanged - VehicleGuid: " + VehicleGuid + " PlayerGuid: " + PlayerGuid + " IsPiloting: " + IsPiloting + "]";
        }
    }
}
