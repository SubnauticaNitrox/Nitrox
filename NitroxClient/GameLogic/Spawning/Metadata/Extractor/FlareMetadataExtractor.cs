using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

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
