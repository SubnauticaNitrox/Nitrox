using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CrashHomeMetadataExtractor : EntityMetadataExtractor<CrashHome, CrashHomeMetadata>
{
    public override CrashHomeMetadata Extract(CrashHome crashHome)
    {
        return new(crashHome.spawnTime);
    }
}
