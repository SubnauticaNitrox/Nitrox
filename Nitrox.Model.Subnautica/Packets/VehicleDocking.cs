using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class VehicleDocking : Packet
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
