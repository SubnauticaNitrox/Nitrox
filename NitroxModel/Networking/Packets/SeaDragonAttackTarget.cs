using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SeaDragonAttackTarget : Packet
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
