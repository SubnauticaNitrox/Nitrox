using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class CrafterMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public NitroxTechType TechType { get; }

    [DataMember(Order = 2)]
    public float StartTime { get; }

    [DataMember(Order = 3)]
    public float Duration { get; }

    [DataMember(Order = 4)]
    public int Amount { get; }

    [DataMember(Order = 5)]
    public int LinkedIndex { get; }

    [IgnoreConstructor]
    protected CrafterMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrafterMetadata(NitroxTechType techType, float startTime, float duration, int amount, int linkedIndex)
    {
        TechType = techType;
        StartTime = startTime;
        Duration = duration;
        Amount = amount;
        LinkedIndex = linkedIndex;
    }

    public override string ToString()
    {
        return $"[{nameof(CrafterMetadata)} TechType: {TechType}, StartTime: {StartTime}, Duration: {Duration}, Amount: {Amount}, LinkedIndex: {LinkedIndex}]";
    }
}
