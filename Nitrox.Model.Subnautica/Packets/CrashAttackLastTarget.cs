using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CrashAttackLastTarget : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxId TargetId { get; }

    public CrashAttackLastTarget(NitroxId creatureId, NitroxId targetId)
    {
        CreatureId = creatureId;
        TargetId = targetId;
    }
}
