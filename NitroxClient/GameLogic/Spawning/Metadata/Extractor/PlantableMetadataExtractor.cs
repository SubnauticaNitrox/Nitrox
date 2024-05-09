using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlantableMetadataExtractor : EntityMetadataExtractor<Plantable, PlantableMetadata>
{
    public override PlantableMetadata Extract(Plantable plantable)
    {
        return plantable.growingPlant ? new(plantable.growingPlant.timeStartGrowth) : null;
    }
}
