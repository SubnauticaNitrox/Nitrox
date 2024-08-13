using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class SeaDragonGrabExosuit : Packet
{
    public NitroxId SeaDragonId { get; }
    public NitroxId TargetId { get; }

    public SeaDragonGrabExosuit(NitroxId seaDragonId, NitroxId targetId)
    {
        SeaDragonId = seaDragonId;
        TargetId = targetId;
    }
}
