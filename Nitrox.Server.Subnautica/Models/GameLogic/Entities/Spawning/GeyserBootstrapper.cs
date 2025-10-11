using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

public class GeyserBootstrapper : IEntityBootstrapper
{
    public void Prepare(ref WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        entity = new GeyserWorldEntity(entity.Transform, entity.Level, entity.ClassId,
                                  entity.SpawnedByServer, entity.Id, entity.TechType,
                                  entity.Metadata, entity.ParentId, entity.ChildEntities,
                                  XorRandom.NextFloat(), 15 * XorRandom.NextFloat());
        // The value 15 doesn't mean anything in particular, it's just an initial eruption time window so geysers don't all erupt at the same time at first
    }
}
