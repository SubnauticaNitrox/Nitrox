using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record AttackCyclopsTargetChanged : Packet
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
