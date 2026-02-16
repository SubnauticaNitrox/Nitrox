using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class VehicleUndocking : Packet
{
    public NitroxId VehicleId { get; }
    public NitroxId DockId { get; }
    public SessionId SessionId { get; }
    public bool UndockingStart { get; }

    public VehicleUndocking(NitroxId vehicleId, NitroxId dockId, SessionId sessionId, bool undockingStart)
    {
        VehicleId = vehicleId;
        DockId = dockId;
        SessionId = sessionId;
        UndockingStart = undockingStart;
    }
}
