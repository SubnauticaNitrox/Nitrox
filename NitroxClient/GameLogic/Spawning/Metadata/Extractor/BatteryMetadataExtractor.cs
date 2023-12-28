using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BatteryMetadataExtractor : EntityMetadataExtractor<Battery, BatteryMetadata>
{
    public override BatteryMetadata Extract(Battery entity)
    {
        return new(entity._charge);
    }
}
