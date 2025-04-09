using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class IOxygenSourceMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float AvailableOxygen { get; set; }

    [IgnoreConstructor]
    protected IOxygenSourceMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public IOxygenSourceMetadata(float availableOxygen)
    {
        AvailableOxygen = availableOxygen;
    }

    public override string ToString()
    {
        return $"[IOxygenSourceMetadata AvailableOxygen: {AvailableOxygen}]";
    }
}
