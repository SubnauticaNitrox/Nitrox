using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class KeypadMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool Unlocked { get; }

    public KeypadMetadata(bool unlocked)
    {
        Unlocked = unlocked;
    }

    public override string ToString()
    {
        return $"[KeypadMetadata - Unlocked: {Unlocked}]";
    }
}
