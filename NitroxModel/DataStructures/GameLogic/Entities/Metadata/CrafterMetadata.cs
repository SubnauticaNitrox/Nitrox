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
    public float StartTime { get; }

    [DataMember(Order = 3)]
    public float Duration { get; }

    [IgnoreConstructor]
    protected CrafterMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CrafterMetadata(NitroxTechType techType, float startTime, float duration)
    {
        TechType = techType;
        StartTime = startTime;
        Duration = duration;
    }

    public override string ToString()
    {
        return $"[CrafterMetadata TechType: {TechType} StartTime: {StartTime}  Duration: {Duration} {base.ToString()}]";
    }
}
