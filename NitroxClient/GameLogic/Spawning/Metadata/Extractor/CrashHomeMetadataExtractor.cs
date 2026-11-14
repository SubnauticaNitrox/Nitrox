using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CrashHomeMetadataExtractor : EntityMetadataExtractor<CrashHome, CrashHomeMetadata>
{
    public override CrashHomeMetadata Extract(CrashHome crashHome)
    {
        Optional<NitroxId> spawnedCrashId = Optional.Empty;
        if (crashHome.crash && crashHome.crash.TryGetNitroxId(out NitroxId crashId))
        {
            spawnedCrashId = crashId;
        }
        return new(crashHome.spawnTime, spawnedCrashId);
    }
}
