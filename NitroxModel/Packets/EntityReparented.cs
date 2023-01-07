using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class EntityReparented : Packet
{
    public NitroxId Id { get; }

    public NitroxId NewParentId { get; }

    public EntityReparented(NitroxId id, NitroxId newParentId)
    {
        Id = id;
        NewParentId = newParentId;
    }
}
