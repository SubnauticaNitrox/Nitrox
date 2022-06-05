using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class PrecursorDoorwayMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool IsOpen { get; }

    public PrecursorDoorwayMetadata(bool isOpen)
    {
        IsOpen = isOpen;
    }

    public override string ToString()
    {
        return $"[PrecursorDoorwayMetadata - IsOpen: {IsOpen}]";
    }
}
