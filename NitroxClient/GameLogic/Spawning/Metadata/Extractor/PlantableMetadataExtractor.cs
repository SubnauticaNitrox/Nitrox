using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlantableMetadataExtractor : GenericEntityMetadataExtractor<Plantable, PlantableMetadata>
{
    public override PlantableMetadata Extract(Plantable entity)
    {
        GrowingPlant growingPlant = entity.growingPlant;

        return new(growingPlant.GetProgress());
    }
}
