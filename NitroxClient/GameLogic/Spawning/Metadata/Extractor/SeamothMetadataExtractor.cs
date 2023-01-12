using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SeamothMetadataExtractor : GenericEntityMetadataExtractor<SeaMoth, SeamothMetadata>
{
    public override SeamothMetadata Extract(SeaMoth seamoth)
    {
        bool lightsOn = (seamoth.toggleLights) ? seamoth.toggleLights.GetLightsActive() : true;

        return new(lightsOn);
    }
}
