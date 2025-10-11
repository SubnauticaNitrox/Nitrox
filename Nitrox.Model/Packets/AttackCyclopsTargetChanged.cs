using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class AttackCyclopsTargetChanged : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxId TargetId { get; }
    public float AggressiveToNoiseAmount { get; }

    public AttackCyclopsTargetChanged(NitroxId creatureId, NitroxId targetId, float aggressiveToNoiseAmount)
    {
        CreatureId = creatureId;
        TargetId = targetId;
        AggressiveToNoiseAmount = aggressiveToNoiseAmount;
    }
}
