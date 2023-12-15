using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FlareMetadataExtractor : GenericEntityMetadataExtractor<Flare, FlareMetadata>
{
    private readonly Items items;

    public FlareMetadataExtractor(Items items)
    {
        this.items = items;
    }

    public override FlareMetadata Extract(Flare flare)
    {
        // If the flare is thrown, set its activation time
        if (flare.flareActiveState && items.PickingUpObject != flare.gameObject)
        {
            return new(flare.energyLeft, flare.hasBeenThrown, flare.flareActivateTime);
        }
        return new(flare.energyLeft, flare.hasBeenThrown, null);
    }
}
