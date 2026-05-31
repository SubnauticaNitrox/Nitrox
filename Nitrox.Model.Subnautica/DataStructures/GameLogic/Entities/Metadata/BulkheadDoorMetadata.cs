using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class BulkheadDoorMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Opened { get; }

    [IgnoreConstructor]
    protected BulkheadDoorMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BulkheadDoorMetadata(bool opened)
    {
        Opened = opened;
    }

    public override string ToString()
    {
        return $"[{nameof(BulkheadDoorMetadata)} Opened: {Opened}]";
    }
}
