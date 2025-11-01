using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DropSimulationOwnership : Packet
{
    public NitroxId EntityId { get; set; }

    public DropSimulationOwnership(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
