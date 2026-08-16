using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class EscapePodMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool PodRepaired { get; }

    [DataMember(Order = 2)]
    public bool RadioRepaired { get; }

    [DataMember(Order = 3)]
    public bool BottomHatchUsed { get; }

    [DataMember(Order = 4)]
    public bool TopHatchUsed { get; }

    [IgnoreConstructor]
    protected EscapePodMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public EscapePodMetadata(bool podRepaired, bool radioRepaired, bool bottomHatchUsed, bool topHatchUsed)
    {
        PodRepaired = podRepaired;
        RadioRepaired = radioRepaired;
        BottomHatchUsed = bottomHatchUsed;
        TopHatchUsed = topHatchUsed;
    }

    public override string ToString()
    {
        return $"[{nameof(EscapePodMetadata)} - PodRepaired: {PodRepaired}, RadioRepaired: {RadioRepaired}, BottomHatchUsed: {BottomHatchUsed}, TopHatchUsed: {TopHatchUsed}]";
    }
}
