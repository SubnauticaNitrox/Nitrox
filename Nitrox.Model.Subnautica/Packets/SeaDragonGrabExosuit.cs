using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
