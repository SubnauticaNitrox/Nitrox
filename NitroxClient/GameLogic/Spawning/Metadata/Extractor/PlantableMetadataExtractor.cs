using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlantableMetadataExtractor(FruitPlantMetadataExtractor fruitPlantMetadataExtractor) : EntityMetadataExtractor<Plantable, PlantableMetadata>
{
    private readonly FruitPlantMetadataExtractor fruitPlantMetadataExtractor = fruitPlantMetadataExtractor;

    public override PlantableMetadata Extract(Plantable plantable)
    {
        // Default value for no progress is -1
        PlantableMetadata metadata = new(plantable.growingPlant ? plantable.growingPlant.timeStartGrowth : -1, plantable.GetSlotID());

        // TODO: Refer to the TODO in PlantableMetadata
        if (plantable.linkedGrownPlant && plantable.linkedGrownPlant.TryGetComponent(out FruitPlant fruitPlant))
        {
            metadata.FruitPlantMetadata = fruitPlantMetadataExtractor.Extract(fruitPlant);
        }

        return metadata;
    }
}
