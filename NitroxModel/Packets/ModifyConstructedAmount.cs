using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class ModifyConstructedAmount : Packet
{
    public NitroxId GhostId { get; }
    public float ConstructedAmount { get; }

    public ModifyConstructedAmount(NitroxId ghostId, float constructedAmount)
    {
        GhostId = ghostId;
        ConstructedAmount = constructedAmount;
    }
}
