using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class PrecursorTeleporterMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool IsOpen { get; }

    public PrecursorTeleporterMetadata(bool isOpen)
    {
        IsOpen = isOpen;
    }

    public override string ToString()
    {
        return $"[PrecursorTeleporterMetadata - IsOpen: {IsOpen}]";
    }
}
