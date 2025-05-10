using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed record ModifyConstructedAmount : Packet
{
    public NitroxId GhostId { get; }
    public float ConstructedAmount { get; }

    public ModifyConstructedAmount(NitroxId ghostId, float constructedAmount)
    {
        GhostId = ghostId;
        ConstructedAmount = constructedAmount;
    }
}
