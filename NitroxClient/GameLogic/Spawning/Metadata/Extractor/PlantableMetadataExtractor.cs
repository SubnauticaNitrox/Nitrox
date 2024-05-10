using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlantableMetadataExtractor : EntityMetadataExtractor<Plantable, PlantableMetadata>
{
    public override PlantableMetadata Extract(Plantable plantable)
    {
        return new(plantable.growingPlant ? plantable.growingPlant.timeStartGrowth : 0, plantable.GetSlotID());
    }
}
