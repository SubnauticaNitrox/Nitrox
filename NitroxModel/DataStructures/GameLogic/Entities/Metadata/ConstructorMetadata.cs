using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[ProtoContract]
public class ConstructorMetadata : EntityMetadata
{
    [ProtoMember(1)]
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
}

