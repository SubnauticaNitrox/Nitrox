using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

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
