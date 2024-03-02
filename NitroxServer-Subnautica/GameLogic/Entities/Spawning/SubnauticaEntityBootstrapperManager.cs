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
    };
    private static readonly Dictionary<string, IEntityBootstrapper> entityBootstrappersByClassId = new()
    {
        ["ce0b4131-86e2-444b-a507-45f7b824a286"] = new GeyserBootstrapper(),
        ["8d3d3c8b-9290-444a-9fea-8e5493ecd6fe"] = new ReefbackBootstrapper()
    };

    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        if (entityBootstrappersByTechType.TryGetValue(spawnedEntity.TechType, out IEntityBootstrapper bootstrapper) ||
            (!string.IsNullOrEmpty(spawnedEntity.ClassId) && entityBootstrappersByClassId.TryGetValue(spawnedEntity.ClassId, out bootstrapper)))
        {
            bootstrapper.Prepare(ref spawnedEntity, generator);
        }
    }
}
