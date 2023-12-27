using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning;

public class GeyserBootstrapper : IEntityBootstrapper
{
    public void Prepare(WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        if (entity is GeyserEntity geyserEntity)
        {
            geyserEntity.RandomIntervalVarianceMultiplier = XORRandom.NextFloat();
            // The value 15 doesn't mean anything in particular, it's just an initial eruption time window so geysers don't all erupt at the same time at first
            geyserEntity.StartEruptTime = 15 * XORRandom.NextFloat();
        }
    }
}
