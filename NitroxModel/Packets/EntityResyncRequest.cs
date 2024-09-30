using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class EntityResyncRequest : Packet
{
    public List<NitroxId> RequestedIds { get; }

    public EntityResyncRequest(List<NitroxId> requestedIds)
    {
        RequestedIds = requestedIds;
    }
}
