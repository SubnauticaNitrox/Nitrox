using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BulkheadDoorMetadataExtractor : EntityMetadataExtractor<BulkheadDoor, BulkheadDoorMetadata>
{
    public override BulkheadDoorMetadata Extract(BulkheadDoor door)
    {
        Log.Info($"[BulkheadDoorMetadataExtractor] state={door.opened}");
        return new BulkheadDoorMetadata(door.opened);
    }
}
