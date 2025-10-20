using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class BeaconMetadataExtractor : EntityMetadataExtractor<Beacon, BeaconMetadata>
{
    public override BeaconMetadata Extract(Beacon beacon)
    {
        return new(beacon.beaconLabel.GetLabel());
    }
}
