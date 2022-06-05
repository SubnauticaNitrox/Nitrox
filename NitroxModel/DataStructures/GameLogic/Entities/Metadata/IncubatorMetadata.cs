using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class IncubatorMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public bool Powered { get; }

    [JsonMemberTransition]
    public bool Hatched { get; }

    public IncubatorMetadata(bool powered, bool hatched)
    {
        Powered = powered;
        Hatched = hatched;
    }

    public override string ToString()
    {
        return $"[IncubatorMetadata - Powered: {Powered} Hatched: {Hatched}]";
    }
}
