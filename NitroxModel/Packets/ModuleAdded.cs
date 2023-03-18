using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class ModuleAdded : Packet
{
    public NitroxId Id { get; }

    public NitroxId ParentId { get; }

    public string Slot { get; }

    public ModuleAdded(NitroxId id, NitroxId parentId, string slot)
    {
        Id = id;
        ParentId = parentId;
        Slot = slot;
    }
}
