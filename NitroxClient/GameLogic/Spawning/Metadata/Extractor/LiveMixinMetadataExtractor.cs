using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

/// <summary>
/// Vehicles/cyclops already sync health through their own metadata.
/// </summary>
public class LiveMixinMetadataExtractor : EntityMetadataExtractor<LiveMixin, LiveMixinMetadata>
{
    public override LiveMixinMetadata Extract(LiveMixin liveMixin)
    {
        if (Resolve<LiveMixinManager>().IsWhitelistedUpdateType(liveMixin))
        {
            return null;
        }

        return new(liveMixin.health);
    }
}
