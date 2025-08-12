using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;

public class CrashHomeBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        // Set 0 for spawnTime so that CrashHome.Update can spawn a Crash if Start() couldn't
        entity.Metadata = new CrashHomeMetadata(0);
    }
}
