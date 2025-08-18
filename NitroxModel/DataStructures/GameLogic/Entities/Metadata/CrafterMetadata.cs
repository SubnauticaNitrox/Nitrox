using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class CrafterMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public NitroxTechType TechType { get; }

    [DataMember(Order = 2)]
    public int Amount { get; }

    [DataMember(Order = 3)]
    public float StartTime { get; }

    [DataMember(Order = 4)]
    public float Duration { get; }

    [IgnoreConstructor]
    protected CrafterMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrafterMetadata(NitroxTechType techType, int amount, float startTime, float duration)
    {
        TechType = techType;
        Amount = amount;
        StartTime = startTime;
        Duration = duration;
    }

    public override string ToString()
    {
        return $"[{nameof(CrafterMetadata)} TechType: {TechType}, Amount: {Amount}, StartTime: {StartTime}, Duration: {Duration}]";
    }
}
