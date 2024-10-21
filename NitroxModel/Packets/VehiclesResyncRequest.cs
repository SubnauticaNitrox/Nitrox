using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class VehiclesResyncRequest : Packet
{
    public NitroxId EntityId { get; }
    public bool ResyncEverything { get; }

    public VehiclesResyncRequest(NitroxId entityId = null, bool resyncEverything = true)
    {
        EntityId = entityId;
        ResyncEverything = resyncEverything;
    }
}
