using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ClearPlanterProcessor(EntityRegistry entityRegistry, ILogger<ClearPlanterProcessor> logger) : IAuthPacketProcessor<ClearPlanter>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<ClearPlanterProcessor> logger = logger;

    public Task Process(AuthProcessorContext context, ClearPlanter packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.PlanterId, out PlanterEntity planterEntity))
        {
            logger.ZLogErrorOnce($"could not find {nameof(PlanterEntity)} with id {packet.PlanterId}");
            return Task.CompletedTask;
        }

        // No need to transmit this packet since the operation is automatically done on remote clients
        entityRegistry.CleanChildren(planterEntity);
        return Task.CompletedTask;
    }
}
