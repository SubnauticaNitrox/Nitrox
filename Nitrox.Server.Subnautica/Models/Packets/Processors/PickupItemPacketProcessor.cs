using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PickupItemPacketProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData)
    : IAuthPacketProcessor<PickupItem>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public async Task Process(AuthProcessorContext context, PickupItem packet)
    {
        NitroxId id = packet.Item.Id;
        if (simulationOwnershipData.RevokeOwnerOfId(id))
        {
            ushort serverId = ushort.MaxValue;
            SimulationOwnershipChange simulationOwnershipChange = new(id, serverId, SimulationLockType.TRANSIENT);
            await context.ReplyToAll(simulationOwnershipChange);
        }

        StopTrackingExistingWorldEntity(id);

        entityRegistry.AddOrUpdate(packet.Item);

        // Have other players respawn the item inside the inventory.
        await context.ReplyToOthers(new SpawnEntities(packet.Item, forceRespawn: true));
    }

    private void StopTrackingExistingWorldEntity(NitroxId id)
    {
        Optional<Entity> entity = entityRegistry.GetEntityById(id);

        if (entity is { HasValue: true, Value: WorldEntity worldEntity })
        {
            // Do not track this entity in the open world anymore.
            worldEntityManager.StopTrackingEntity(worldEntity);
        }
    }
}
