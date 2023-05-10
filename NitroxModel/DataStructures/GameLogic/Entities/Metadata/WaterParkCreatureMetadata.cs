using BinaryPack.Attributes;
using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class WaterParkCreatureMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Age;

    [DataMember(Order = 2)]
    public double MatureTime;

    [DataMember(Order = 3)]
    public float TimeNextBreed;
    
    [DataMember(Order = 4)]
    public bool BornInside;

    [IgnoreConstructor]
    protected WaterParkCreatureMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public WaterParkCreatureMetadata(float age, double matureTime, float timeNextBreed, bool bornInside)
    {
        Age = age;
        MatureTime = matureTime;
        TimeNextBreed = timeNextBreed;
        BornInside = bornInside;
    }

    public override string ToString()
    {
        return $"[WaterParkCreatureMetadata Age: {Age}, MatureTime: {MatureTime}, TimeNextBreed: {TimeNextBreed}, BornInside: {BornInside}]";
    }
}
