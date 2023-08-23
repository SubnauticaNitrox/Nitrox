using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BeaconMetadataExtractor : GenericEntityMetadataExtractor<BeaconLabel, BeaconMetadata>
{
    public override BeaconMetadata Extract(BeaconLabel beaconLabel)
    {
        return new(beaconLabel.GetLabel());
    }
}
