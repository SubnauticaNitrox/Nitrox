using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public string VehicleGuid { get; }
        public ulong LPlayerId { get; }
        public bool IsPiloting { get; }

        public VehicleOnPilotModeChanged(string vehicleGuid, ulong playerId, bool isPiloting)
        {
            VehicleGuid = vehicleGuid;
            LPlayerId = playerId;
            IsPiloting = isPiloting;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotModeChanged - VehicleGuid: " + VehicleGuid + " PlayerId: " + LPlayerId + " IsPiloting: " + IsPiloting + "]";
        }
    }
}
