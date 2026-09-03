using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

internal sealed class GeyserBootstrapper(XorRandom random) : IEntityBootstrapper
{
    private readonly XorRandom random = random;

    public void Prepare(ref WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        GeyserWorldEntity geyserEntity = new(entity.Transform, entity.Level, entity.ClassId,
                                  entity.SpawnedByServer, entity.Id, entity.TechType,
                                  null, entity.ParentId, entity.ChildEntities,
                                  random.NextFloat(), 15 * random.NextFloat());
        // The value 15 doesn't mean anything in particular, it's just an initial eruption time window so geysers don't all erupt at the same time at first
        geyserEntity.Metadata = entity.Metadata;
        entity = geyserEntity;
    }
}
