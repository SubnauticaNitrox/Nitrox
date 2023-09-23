using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SeamothMetadataExtractor : GenericEntityMetadataExtractor<SeaMoth, SeamothMetadata>
{
    public override SeamothMetadata Extract(SeaMoth seamoth) => ExtractStatic(seamoth);

    // DI give problems to use the instance of this class directly inside patches. MetadataExtractor is partial over-designed and probably needs a rework.
    public static SeamothMetadata ExtractStatic(SeaMoth seamoth)
    {
        bool lightsOn = seamoth.toggleLights ? seamoth.toggleLights.GetLightsActive() : true;
        float health = seamoth.liveMixin.health;

        return new(lightsOn, health);
    }
}
