using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class EquippedItemData : ItemData
{
    [JsonMemberTransition]
    public string Slot { get; }

    [JsonMemberTransition]
    public NitroxTechType TechType { get; }

    public EquippedItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, string slot, NitroxTechType techType) : base(containerId, itemId, serializedData)
    {
        Slot = slot;
        TechType = techType;
    }

    public override string ToString()
    {
        return $"[EquippedItemData - {base.ToString()}, Slot: {Slot}, TechType: {TechType}]";
    }
}
