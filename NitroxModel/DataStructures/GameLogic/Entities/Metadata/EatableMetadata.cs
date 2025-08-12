using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class EatableMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float TimeDecayStart { get; }

    [IgnoreConstructor]
    protected EatableMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }
    public EatableMetadata(float timeDecayStart)
    {
        TimeDecayStart = timeDecayStart;
    }

    public override string ToString()
    {
        return $"[{nameof(EatableMetadata)} TimeDecayStart: {TimeDecayStart}]";
    }
}
