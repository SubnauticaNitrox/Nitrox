using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class FlashlightMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool On { get; }

    [IgnoreConstructor]
    protected FlashlightMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public FlashlightMetadata(bool on)
    {
        On = on;
    }

    public override string ToString()
    {
        return $"[FlashlightMetadata On: {On}]";
    }
}
