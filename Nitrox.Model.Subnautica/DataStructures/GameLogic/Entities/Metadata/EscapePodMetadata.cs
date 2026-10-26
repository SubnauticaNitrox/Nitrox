using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.Core;

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
    public List<SessionId> PlayersWithBottomHatchUsed { get; } = [];

    [DataMember(Order = 4)]
    public List<SessionId> PlayersWithTopHatchUsed { get; } = [];

    [IgnoreConstructor]
    protected EscapePodMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public EscapePodMetadata(bool podRepaired, bool radioRepaired, List<SessionId> playersWithBottomHatchUsed, List<SessionId> playersWithTopHatchUsed)
    {
        PodRepaired = podRepaired;
        RadioRepaired = radioRepaired;
        PlayersWithBottomHatchUsed = playersWithBottomHatchUsed;
        PlayersWithTopHatchUsed = playersWithTopHatchUsed;
    }

    public override string ToString()
    {
        return $"[{nameof(EscapePodMetadata)} - PodRepaired: {PodRepaired}, RadioRepaired: {RadioRepaired}, PlayersWithBottomHatchUsed: {PlayersWithBottomHatchUsed.Count}, PlayersWithTopHatchUsed: {PlayersWithTopHatchUsed.Count}]";
    }
}
