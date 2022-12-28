using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[ProtoContract]
public class RepairedComponentMetadata : EntityMetadata
{
    [ProtoMember(1)]
    public NitroxTechType TechType { get; }

    [IgnoreConstructor]
    protected RepairedComponentMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public RepairedComponentMetadata(NitroxTechType techType)
    {
        TechType = techType;
    }

    public override string ToString()
    {
        return $"[RepairedComponentMetadata - TechType: {TechType}]";
    }
}
