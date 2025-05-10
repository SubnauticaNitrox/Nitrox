using Nitrox.Server.Subnautica.Models.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class GeyserBootstrapper : IEntityBootstrapper
{
    private readonly SubnauticaServerRandom random;

    public GeyserBootstrapper(SubnauticaServerRandom random)
    {
        this.random = random;
    }

    public void Prepare(ref WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        entity = new GeyserWorldEntity(entity.Transform, entity.Level, entity.ClassId,
                                  entity.SpawnedByServer, entity.Id, entity.TechType,
                                  entity.Metadata, entity.ParentId, entity.ChildEntities,
                                  random.NextFloat(), 15 * random.NextFloat());
        // The value 15 doesn't mean anything in particular, it's just an initial eruption time window so geysers don't all erupt at the same time at first
    }
}
