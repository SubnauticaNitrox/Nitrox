using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class StayAtLeashPositionMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public NitroxVector3 LeashPosition { get; }

    [IgnoreConstructor]
    protected StayAtLeashPositionMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public StayAtLeashPositionMetadata(NitroxVector3 leashPosition)
    {
        LeashPosition = leashPosition;
    }

    public override string ToString()
    {
        return $"[{nameof(StayAtLeashPositionMetadata)} LeashPosition: {LeashPosition}]";
    }
}
