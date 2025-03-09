using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class DrillableMetadataExtractor : EntityMetadataExtractor<Drillable, DrillableMetadata>
{
    public override DrillableMetadata Extract(Drillable entity)
    {
        return new(entity.health, entity.timeLastDrilled);
    }
}
