using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class PrecursorTeleporterActivationTerminalMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool Unlocked { get; }

    public PrecursorTeleporterActivationTerminalMetadata(bool unlocked)
    {
        Unlocked = unlocked;
    }

    public override string ToString()
    {
        return $"[PrecursorTeleporterActivationTerminalMetadata - Unlocked: {Unlocked}]";
    }
}
