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
#if SUBNAUTICA
    public AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount)
#elif BELOWZERO
    public float TargetPriority { get; }

    public AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount, float targetPriority)
#endif
    {
        CreatureId = creatureId;
        TargetId = targetId;
        Locked = locked;
        AggressionAmount = aggressionAmount;
#if BELOWZERO
        TargetPriority = targetPriority;
#endif
    }
}
