using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleOnPilotModeChanged : Packet
    {
        public NitroxId VehicleId { get; }
        public ushort PlayerId { get; }
        public bool IsPiloting { get; }

        public VehicleOnPilotModeChanged(NitroxId vehicleId, ushort playerId, bool isPiloting)
        {
            VehicleId = vehicleId;
            PlayerId = playerId;
            IsPiloting = isPiloting;
        }
    }
}
