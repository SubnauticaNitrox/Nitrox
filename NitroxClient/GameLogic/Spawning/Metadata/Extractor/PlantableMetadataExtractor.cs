using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlantableMetadataExtractor : EntityMetadataExtractor<Plantable, PlantableMetadata>
{
    public override PlantableMetadata Extract(Plantable entity)
    {
        GrowingPlant growingPlant = entity.growingPlant;

        // The growing plant will only spawn in the proper container. In other containers, just consider progress as 0.
        float progress = (growingPlant != null) ? growingPlant.GetProgress() : 0; 

        return new(progress);
    }
}
