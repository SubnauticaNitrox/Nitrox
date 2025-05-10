using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record DropSimulationOwnership : Packet
{
    public NitroxId EntityId { get; set; }

    public DropSimulationOwnership(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
