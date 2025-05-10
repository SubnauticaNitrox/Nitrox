using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class SubnauticaEntityBootstrapperManager : IEntityBootstrapperManager
{
    private readonly Dictionary<string, IEntityBootstrapper> entityBootstrappersByClassId;

    private readonly Dictionary<NitroxTechType, IEntityBootstrapper> entityBootstrappersByTechType;

    public SubnauticaEntityBootstrapperManager(SubnauticaServerRandom random)
    {
        entityBootstrappersByTechType = new()
        {
            [TechType.CrashHome.ToDto()] = new CrashHomeBootstrapper(),
            [TechType.ReaperLeviathan.ToDto()] = new StayAtLeashPositionBootstrapper(),
            [TechType.SeaDragon.ToDto()] = new StayAtLeashPositionBootstrapper(),
            [TechType.GhostLeviathan.ToDto()] = new StayAtLeashPositionBootstrapper(),
        };
        entityBootstrappersByClassId = new()
        {
            ["ce0b4131-86e2-444b-a507-45f7b824a286"] = new GeyserBootstrapper(random),
            ["8d3d3c8b-9290-444a-9fea-8e5493ecd6fe"] = new ReefbackBootstrapper(random)
        };
    }

    public void PrepareEntityIfRequired(ref WorldEntity spawnedEntity, DeterministicGenerator generator)
    {
        if (entityBootstrappersByTechType.TryGetValue(spawnedEntity.TechType, out IEntityBootstrapper bootstrapper) ||
            (!string.IsNullOrEmpty(spawnedEntity.ClassId) && entityBootstrappersByClassId.TryGetValue(spawnedEntity.ClassId, out bootstrapper)))
        {
            bootstrapper.Prepare(ref spawnedEntity, generator);
        }
    }
}
