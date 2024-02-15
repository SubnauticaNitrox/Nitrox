using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class RangedAttackLastTargetUpdate : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxId TargetId { get; }
    public int AttackTypeIndex { get; }
    public ActionState State { get; }

    public RangedAttackLastTargetUpdate(NitroxId creatureId, NitroxId targetId, int attackTypeIndex, ActionState state)
    {
        CreatureId = creatureId;
        TargetId = targetId;
        AttackTypeIndex = attackTypeIndex;
        State = state;
    }

    public enum ActionState
    {
        CHARGING,
        CASTING
    }
}
