using System;
using System.Collections.Generic;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
///     Entity tech progress. Stores per unique scannable entity.
/// </summary>
[Serializable]
[JsonContractTransition]
public class PDAProgressEntry
{
    [JsonMemberTransition]
    public NitroxTechType TechType { get; set; }

    [JsonMemberTransition]
    public Dictionary<NitroxId, float> Entries { get; set; }

    public PDAProgressEntry(NitroxTechType techType, Dictionary<NitroxId, float> entries)
    {
        TechType = techType;
        Entries = entries;
    }
}
