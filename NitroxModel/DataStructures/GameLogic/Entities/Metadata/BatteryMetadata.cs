using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[ProtoContract]
public class BatteryMetadata : EntityMetadata
{
    [ProtoMember(1)]
    public float Charge { get; }

    [IgnoreConstructor]
    protected BatteryMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public BatteryMetadata(float charge)
    {
        Charge = charge;
    }

    public override string ToString()
    {
        return $"[BatteryMetadata Charge: {Charge}]";
    }
}

