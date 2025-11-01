using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BatteryMetadataExtractor : EntityMetadataExtractor<Battery, BatteryMetadata>
{
    public override BatteryMetadata Extract(Battery entity)
    {
        return new(entity._charge);
    }
}
