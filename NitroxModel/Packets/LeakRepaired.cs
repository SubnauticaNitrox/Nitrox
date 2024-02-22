using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class LeakRepaired : Packet
{
    public NitroxId BaseId { get; set; }
    public NitroxId LeakId { get; set; }
    public NitroxInt3 RelativeCell { get; set; }

    public LeakRepaired(NitroxId baseId, NitroxId leakId, NitroxInt3 relativeCell)
    {
        BaseId = baseId;
        LeakId = leakId;
        RelativeCell = relativeCell;
    }
}
