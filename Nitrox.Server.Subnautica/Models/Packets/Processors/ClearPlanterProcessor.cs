using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ClearPlanterProcessor : AuthenticatedPacketProcessor<ClearPlanter>
{
    private readonly EntityRegistry entityRegistry;

    public ClearPlanterProcessor(EntityRegistry entityRegistry)
    {
        this.entityRegistry = entityRegistry;
    }

    public override void Process(ClearPlanter packet, Player player)
    {
        if (entityRegistry.TryGetEntityById(packet.PlanterId, out PlanterEntity planterEntity))
        {
            // No need to transmit this packet since the operation is automatically done on remote clients
            entityRegistry.CleanChildren(planterEntity);
        }
        else
        {
            Log.ErrorOnce($"[{nameof(ClearPlanterProcessor)}] Could not find PlanterEntity with id {packet.PlanterId}");
        }
    }
}
