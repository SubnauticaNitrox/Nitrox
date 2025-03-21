using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class EscapePodMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool PodRepaired { get; }

    [DataMember(Order = 2)]
    public bool RadioRepaired { get; }

    [IgnoreConstructor]
    protected EscapePodMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public EscapePodMetadata(bool podRepaired, bool radioRepaired)
    {
        PodRepaired = podRepaired;
        RadioRepaired = radioRepaired;
    }

    public override string ToString()
    {
        return $"[{nameof(EscapePodMetadata)} - PodRepaired: {PodRepaired}, RadioRepaired: {RadioRepaired}]";
    }
}
