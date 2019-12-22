using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public NitroxId VehicleId { get; }
        public NitroxId PlayerId { get; }
        public bool IsPiloting { get; }

        public VehicleOnPilotModeChanged(NitroxId vehicleId, NitroxId playerId, bool isPiloting)
        {
            VehicleId = vehicleId;
            PlayerId = playerId;
            IsPiloting = isPiloting;
        }

        public override string ToString()
        {
            return "[VehicleOnPilotModeChanged - VehicleId: " + VehicleId + " PlayerId: " + PlayerId + " IsPiloting: " + IsPiloting + "]";
        }
    }
}
