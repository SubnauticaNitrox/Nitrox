using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class RepairedComponentMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
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
