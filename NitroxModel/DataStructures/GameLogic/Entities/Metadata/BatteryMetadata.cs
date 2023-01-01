using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class BatteryMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
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
