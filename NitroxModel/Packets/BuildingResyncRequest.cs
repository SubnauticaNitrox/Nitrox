using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets;

[Serializable]
public class BuildingResyncRequest : Packet
{
    public NitroxId EntityId { get; }
    public bool ResyncEverything { get; }

    public BuildingResyncRequest(NitroxId entityId = null, bool resyncEverything = true)
    {
        EntityId = entityId;
        ResyncEverything = resyncEverything;
    }
}
