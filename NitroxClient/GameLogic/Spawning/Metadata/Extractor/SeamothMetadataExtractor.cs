using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SeamothMetadataExtractor : EntityMetadataExtractor<SeaMoth, SeamothMetadata>
{
    public override SeamothMetadata Extract(SeaMoth seamoth)
    {
        bool lightsOn = (seamoth.toggleLights) ? seamoth.toggleLights.GetLightsActive() : true;
        LiveMixin liveMixin = seamoth.liveMixin;
        SubName subName = seamoth.subName;

        return new(lightsOn, liveMixin.health, seamoth.precursorOutOfWater, SubNameInputMetadataExtractor.GetName(subName), SubNameInputMetadataExtractor.GetColors(subName));
    }
}
