using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

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

    public override string ToString()
    {
        return $"ModifyConstructedAmount [GhostId: {GhostId}, ConstructedAmount: {ConstructedAmount}]";
    }
}
