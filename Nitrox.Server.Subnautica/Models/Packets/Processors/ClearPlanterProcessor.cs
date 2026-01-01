using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ClearPlanterProcessor(EntityRegistry entityRegistry, ILogger<ClearPlanterProcessor> logger) : AuthenticatedPacketProcessor<ClearPlanter>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<ClearPlanterProcessor> logger = logger;

    public override void Process(ClearPlanter packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.PlanterId, out PlanterEntity planterEntity))
        {
            logger.ZLogErrorOnce($"could not find {nameof(PlanterEntity)} with id {packet.PlanterId}");
            return;
        }

        // No need to transmit this packet since the operation is automatically done on remote clients
        entityRegistry.CleanChildren(planterEntity);
    }
}
