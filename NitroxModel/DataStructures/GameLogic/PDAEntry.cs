using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class PDAEntry
{
    [JsonMemberTransition]
    public NitroxTechType TechType { get; set; }

    [JsonMemberTransition]
    public float Progress { get; set; }

    [JsonMemberTransition]
    public int Unlocked { get; set; }

    public PDAEntry(NitroxTechType techType, float progress, int unlocked)
    {
        TechType = techType;
        Progress = progress;
        Unlocked = unlocked;
    }
}
