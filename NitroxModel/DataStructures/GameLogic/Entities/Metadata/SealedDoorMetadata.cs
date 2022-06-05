using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class SealedDoorMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool IsSealed { get; }

    [JsonMemberTransition]
    public float OpenedAmount { get; }

    public SealedDoorMetadata(bool isSealed, float openedAmount)
    {
        IsSealed = isSealed;
        OpenedAmount = openedAmount;
    }
}
