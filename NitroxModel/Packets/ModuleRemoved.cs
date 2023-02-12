using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class ModuleRemoved : Packet
{
    public NitroxId Id { get; }

    public NitroxId NewParentId { get; }

    public ModuleRemoved(NitroxId id, NitroxId newParentId)
    {
        Id = id;
        NewParentId = newParentId;
    }
}
