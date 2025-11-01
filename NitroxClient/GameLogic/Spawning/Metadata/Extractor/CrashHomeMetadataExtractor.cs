using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CrashHomeMetadataExtractor : EntityMetadataExtractor<CrashHome, CrashHomeMetadata>
{
    public override CrashHomeMetadata Extract(CrashHome crashHome)
    {
        return new(crashHome.spawnTime);
    }
}
