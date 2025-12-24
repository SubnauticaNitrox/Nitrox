using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FlareMetadataExtractor : EntityMetadataExtractor<Flare, FlareMetadata>
{
    public override FlareMetadata Extract(Flare flare)
    {
        // If the flare is thrown, set its activation time
        if (flare.flareActiveState && Items.PickingUpObject != flare.gameObject)
        {
            return new(flare.energyLeft, flare.hasBeenThrown, flare.flareActivateTime);
        }
        return new(flare.energyLeft, flare.hasBeenThrown, null);
    }
}
