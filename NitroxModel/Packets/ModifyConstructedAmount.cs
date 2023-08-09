using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public sealed class ModifyConstructedAmount : Packet
{
    // TODO: Add resourcemap sync
    public NitroxId GhostId;
    public float ConstructedAmount;

    public ModifyConstructedAmount(NitroxId ghostId, float constructedAmount)
    {
        GhostId = ghostId;
        ConstructedAmount = constructedAmount;
    }
}
