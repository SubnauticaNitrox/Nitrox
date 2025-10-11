using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

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
