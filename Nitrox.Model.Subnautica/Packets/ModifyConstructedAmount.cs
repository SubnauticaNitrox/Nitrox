using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
