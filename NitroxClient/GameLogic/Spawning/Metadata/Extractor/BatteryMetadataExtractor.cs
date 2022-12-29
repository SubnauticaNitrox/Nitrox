using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BatteryMetadataExtractor : GenericEntityMetadataExtractor<Battery, BatteryMetadata>
{
    public override BatteryMetadata Extract(Battery entity)
    {
        return new(entity._charge);
    }
}
