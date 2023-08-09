using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets;

[Serializable]
public sealed class ModifyConstructedAmount : Packet
{
    // TODO: Add resourcemap sync
    public NitroxId GhostId { get; }
    public float ConstructedAmount { get; }

    public ModifyConstructedAmount(NitroxId ghostId, float constructedAmount)
    {
        GhostId = ghostId;
        ConstructedAmount = constructedAmount;
    }
}
