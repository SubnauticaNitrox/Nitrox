using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

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
