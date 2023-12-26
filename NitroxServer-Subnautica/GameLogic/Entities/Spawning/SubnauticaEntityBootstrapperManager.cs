using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning;

public class SubnauticaEntityBootstrapperManager : IEntityBootstrapperManager
{
    private static readonly Dictionary<NitroxTechType, IEntityBootstrapper> entityBootstrappersByTechType = new()
    {
        [TechType.CrashHome.ToDto()] = new CrashHomeBootstrapper(),
        [TechType.Reefback.ToDto()] = new ReefbackBootstrapper()
    };
    private static readonly Dictionary<string, IEntityBootstrapper> entityBootstrappersByClassId = new()
    {
    };

    public void PrepareEntityIfRequired(WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        if (entityBootstrappersByTechType.TryGetValue(spawnedEntity.TechType, out IEntityBootstrapper bootstrapper) ||
            (!string.IsNullOrEmpty(spawnedEntity.ClassId) && entityBootstrappersByClassId.TryGetValue(spawnedEntity.ClassId, out bootstrapper)))
        {
            bootstrapper.Prepare(spawnedEntity, generator);
        }
    }
}
