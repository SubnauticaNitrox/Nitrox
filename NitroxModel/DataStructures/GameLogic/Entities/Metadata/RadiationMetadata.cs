using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class RadiationMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Health { get; set; }

    [DataMember(Order = 2)]
    public float FixRealTime { get; set; }

    [IgnoreConstructor]
    protected RadiationMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public RadiationMetadata(float health, float fixRealTime = -1)
    {
        Health = health;
        FixRealTime = fixRealTime;
    }

    public override string ToString()
    {
        return $"[{nameof(RadiationMetadata)} Health: {Health}, FixRealTime: {FixRealTime}]";
    }
}
