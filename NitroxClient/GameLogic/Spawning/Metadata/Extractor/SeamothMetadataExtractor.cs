using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SeamothMetadataExtractor : VehicleMetadataExtractor<SeaMoth, SeamothMetadata>
{
    public override SeamothMetadata Extract(SeaMoth seamoth)
    {
        bool lightsOn = (seamoth.toggleLights) ? seamoth.toggleLights.GetLightsActive() : true;
        LiveMixin liveMixin = seamoth.liveMixin;
        SubName subName = seamoth.subName;

        return new(lightsOn, GetHealth(liveMixin), GetName(subName), GetColors(subName));
    }
}
