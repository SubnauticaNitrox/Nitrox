using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class VehicleDocking(NitroxId vehicleId, NitroxId dockId, SessionId sessionId) : Packet
{
    public NitroxId VehicleId { get; } = vehicleId;
    public NitroxId DockId { get; } = dockId;
    public SessionId SessionId { get; } = sessionId;
}
