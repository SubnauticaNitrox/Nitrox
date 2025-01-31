using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class EggMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float TimeStartHatching { get; }

    [DataMember(Order = 2)]
    public float Progress { get; }

    [IgnoreConstructor]
    protected EggMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }
    public EggMetadata(float timeStartHatching, float progress)
    {
        TimeStartHatching = timeStartHatching;
        Progress = progress;
    }

    public override string ToString()
    {
        return $"[EggMetadata TimeStartHatching: {TimeStartHatching}, Progress: {Progress}]";
    }
}
