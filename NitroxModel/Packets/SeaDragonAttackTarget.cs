using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class SeaDragonAttackTarget : Packet
{
    public NitroxId SeaDragonId { get; }
    public NitroxId TargetId { get; }
    public float Aggression { get; }

    public SeaDragonAttackTarget(NitroxId seaDragonId, NitroxId targetId, float aggression)
    {
        SeaDragonId = seaDragonId;
        TargetId = targetId;
        Aggression = aggression;
    }
}
