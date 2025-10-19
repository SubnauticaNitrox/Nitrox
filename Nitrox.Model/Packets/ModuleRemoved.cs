using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

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
