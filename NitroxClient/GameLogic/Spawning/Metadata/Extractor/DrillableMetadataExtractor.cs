using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class DrillableMetadataExtractor : EntityMetadataExtractor<Drillable, DrillableMetadata>
{
    public override DrillableMetadata Extract(Drillable entity)
    {
        return new(entity.health, entity.timeLastDrilled);
    }
}
