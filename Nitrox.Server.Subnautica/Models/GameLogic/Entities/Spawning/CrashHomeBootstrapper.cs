using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class CrashHomeBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        // Set 0 for spawnTime so that CrashHome.Update can spawn a Crash if Start() couldn't
        entity.Metadata = new CrashHomeMetadata(0);
    }
}
