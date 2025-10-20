using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CyclopsLightingMetadataExtractor : EntityMetadataExtractor<CyclopsLightingPanel, CyclopsLightingMetadata>
{
    public override CyclopsLightingMetadata Extract(CyclopsLightingPanel lighting)
    {
        return new(lighting.floodlightsOn, lighting.lightingOn);
    }
}
