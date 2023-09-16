using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class BuildingResyncRequest : Packet
{
    public NitroxId EntityId { get; }
    public bool ResyncEverything { get; }

    public BuildingResyncRequest(NitroxId entityId = null, bool resyncEverything = true)
    {
        EntityId = entityId;
        ResyncEverything = resyncEverything;
    }
}
