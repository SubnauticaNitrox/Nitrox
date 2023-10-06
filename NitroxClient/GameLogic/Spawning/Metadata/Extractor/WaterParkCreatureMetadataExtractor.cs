using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class WaterParkCreatureMetadataExtractor : GenericEntityMetadataExtractor<WaterParkCreature, WaterParkCreatureMetadata>
{
    public override WaterParkCreatureMetadata Extract(WaterParkCreature entity)
    {
        // We don't need to save metadata for fishes with default values
        if (entity.age == -1)
        {
            return null;
        }
        return new(entity.age, entity.matureTime, entity.timeNextBreed, entity.bornInside);
    }
}
