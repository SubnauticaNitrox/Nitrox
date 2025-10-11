using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class DropSimulationOwnership : Packet
{
    public NitroxId EntityId { get; set; }

    public DropSimulationOwnership(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
