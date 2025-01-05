using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class DropSimulationOwnership : Packet
{
    public NitroxId EntityId { get; set; }

    public DropSimulationOwnership(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
