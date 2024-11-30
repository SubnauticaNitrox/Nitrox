using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class AggressiveWhenSeeTargetChanged : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxId TargetId { get; }
    public bool Locked { get; }
    public float AggressionAmount { get; }

    public AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount)
    {
        CreatureId = creatureId;
        TargetId = targetId;
        Locked = locked;
        AggressionAmount = aggressionAmount;
    }
}
