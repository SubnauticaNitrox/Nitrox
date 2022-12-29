using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[ProtoContract]
public class FlashlightMetadata : EntityMetadata
{
    [ProtoMember(1)]
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

