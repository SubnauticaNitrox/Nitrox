using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class PrecursorKeyTerminalMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool Slotted { get; }

    public PrecursorKeyTerminalMetadata(bool slotted)
    {
        Slotted = slotted;
    }

    public override string ToString()
    {
        return $"[PrecursorKeyTerminalMetadata - Slotted: {Slotted}]";
    }
}
