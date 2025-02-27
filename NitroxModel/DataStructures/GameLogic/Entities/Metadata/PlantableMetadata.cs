using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PlantableMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float TimeStartGrowth { get; set; }

    [DataMember(Order = 2)]
    public int SlotID { get; set; }

    // TODO: When the metadata system is reworked and we can have multiple metadatas on one entity, this won't be required anymore
    [DataMember(Order = 3)]
    public FruitPlantMetadata FruitPlantMetadata { get; set; }

    [IgnoreConstructor]
    protected PlantableMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlantableMetadata(float timeStartGrowth, int slotID)
    {
        TimeStartGrowth = timeStartGrowth;
        SlotID = slotID;
    }

    public PlantableMetadata(float timeStartGrowth, int slotID, FruitPlantMetadata fruitPlantMetadata)
    {
        TimeStartGrowth = timeStartGrowth;
        SlotID = slotID;
        FruitPlantMetadata = fruitPlantMetadata;
    }

    public override string ToString()
    {
        return $"[{nameof(PlantableMetadata)} TimeStartGrowth: {TimeStartGrowth}, SlotID: {SlotID}, FruitPlantMetadata: {FruitPlantMetadata}]";
    }
}
