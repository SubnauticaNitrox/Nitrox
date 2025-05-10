using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SeaDragonSwatAttack : Packet
{
    public NitroxId SeaDragonId { get; }
    public NitroxId TargetId { get; }
    public bool IsRightHand { get; }
    public float Aggression { get; }

    public SeaDragonSwatAttack(NitroxId seaDragonId, NitroxId targetId, bool isRightHand, float aggression)
    {
        SeaDragonId = seaDragonId;
        TargetId = targetId;
        IsRightHand = isRightHand;
        Aggression = aggression;
    }
}
