using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

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
