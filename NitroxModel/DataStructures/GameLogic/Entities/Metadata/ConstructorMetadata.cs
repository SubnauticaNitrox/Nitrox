using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class ConstructorMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Deployed { get; }

    [IgnoreConstructor]
    protected ConstructorMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }
    public ConstructorMetadata(bool deployed)
    {
        Deployed = deployed;
    }

    public override string ToString()
    {
        return $"[ConstructorMetadata Deployed: {Deployed}]";
    }
}
