using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class SeaTreaderMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool ReverseDirection { get; }

    [DataMember(Order = 2)]
    public float GrazingEndTime { get; }

    [DataMember(Order = 3)]
    public NitroxVector3 LeashPosition { get; }

    [IgnoreConstructor]
    protected SeaTreaderMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public SeaTreaderMetadata(bool reverseDirection, float grazingEndTime, NitroxVector3 leashPosition)
    {
        ReverseDirection = reverseDirection;
        GrazingEndTime = grazingEndTime;
        LeashPosition = leashPosition;
    }

    public override string ToString()
    {
        return $"[{nameof(SeaTreaderMetadata)} ReverseDirection: {ReverseDirection}, GrazingEndTime: {GrazingEndTime}, LeashPosition: {LeashPosition}]";
    }
}
