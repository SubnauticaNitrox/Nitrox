using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class ThermalPlantMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Temperature { get; }

    [IgnoreConstructor]
    protected ThermalPlantMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ThermalPlantMetadata(float temperature)
    {
        Temperature = temperature;
    }

    public override string ToString()
    {
        return $"[{nameof(ThermalPlantMetadata)} {nameof(Temperature)}: {Temperature}]";
    }
}
