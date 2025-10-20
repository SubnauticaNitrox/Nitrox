using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ClearPlanterProcessor : AuthenticatedPacketProcessor<ClearPlanter>
{
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<ClearPlanterProcessor> logger;
    private readonly HashSet<NitroxId> logErrorOnceSet = [];

    public ClearPlanterProcessor(EntityRegistry entityRegistry, ILogger<ClearPlanterProcessor> logger)
    {
        this.entityRegistry = entityRegistry;
        this.logger = logger;
    }

    public override void Process(ClearPlanter packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.PlanterId, out PlanterEntity planterEntity))
        {
            if (!logErrorOnceSet.Contains(packet.PlanterId))
            {
                logger.ZLogError($"[{nameof(ClearPlanterProcessor)}] Could not find PlanterEntity with id {packet.PlanterId}");
                logErrorOnceSet.Add(packet.PlanterId);
            }
            return;
        }

        // No need to transmit this packet since the operation is automatically done on remote clients
        entityRegistry.CleanChildren(planterEntity);
    }
}
