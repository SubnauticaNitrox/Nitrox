using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FlashlightMetadataExtractor : GenericEntityMetadataExtractor<FlashLight, FlashlightMetadata>
{
    public override FlashlightMetadata Extract(FlashLight entity)
    {
        ToggleLights lights = entity.RequireComponent<ToggleLights>();

        return new(lights.lightsActive);
    }
}
