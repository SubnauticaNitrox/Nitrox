using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class VehicleOnPilotModeChanged : Packet
{
    public NitroxId VehicleId { get; }
    public SessionId SessionId { get; }
    public bool IsPiloting { get; }

    public VehicleOnPilotModeChanged(NitroxId vehicleId, SessionId sessionId, bool isPiloting)
    {
        VehicleId = vehicleId;
        SessionId = sessionId;
        IsPiloting = isPiloting;
    }
}
