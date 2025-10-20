using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FlashlightMetadataExtractor : EntityMetadataExtractor<FlashLight, FlashlightMetadata>
{
    public override FlashlightMetadata Extract(FlashLight entity)
    {
        ToggleLights lights = entity.RequireComponent<ToggleLights>();

        return new(lights.lightsActive);
    }
}
