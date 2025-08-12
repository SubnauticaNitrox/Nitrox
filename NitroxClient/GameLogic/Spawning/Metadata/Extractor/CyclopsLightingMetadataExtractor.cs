using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CyclopsLightingMetadataExtractor : EntityMetadataExtractor<CyclopsLightingPanel, CyclopsLightingMetadata>
{
    public override CyclopsLightingMetadata Extract(CyclopsLightingPanel lighting)
    {
        return new(lighting.floodlightsOn, lighting.lightingOn);
    }
}
