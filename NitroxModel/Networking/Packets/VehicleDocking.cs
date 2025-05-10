using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record VehicleDocking : Packet
{
    public NitroxId VehicleId { get; }
    public NitroxId DockId { get; }
    public ushort PlayerId { get; }

    public VehicleDocking(NitroxId vehicleId, NitroxId dockId, ushort playerId)
    {
        VehicleId = vehicleId;
        DockId = dockId;
        PlayerId = playerId;
    }
}
