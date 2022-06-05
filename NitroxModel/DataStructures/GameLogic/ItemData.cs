using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class ItemData
{
    [JsonMemberTransition]
    public NitroxId ContainerId { get; }

    [JsonMemberTransition]
    public NitroxId ItemId { get; }

    [JsonMemberTransition]
    public byte[] SerializedData { get; }

    public ItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData)
    {
        ContainerId = containerId;
        ItemId = itemId;
        SerializedData = serializedData;
    }

    public override string ToString()
    {
        return $"[ItemData - ContainerId: {ContainerId}, Id: {ItemId}, DataLenght: {SerializedData?.Length}]";
    }
}
