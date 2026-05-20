using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using System.Linq;

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
            SimulationOwnershipChange simulationOwnershipChange = new(id, SessionId.SERVER_ID, SimulationLockType.TRANSIENT);
            await context.SendToAllAsync(simulationOwnershipChange);
        }

        PreserveExistingInstalledBatteries(id, packet.Item);

        StopTrackingExistingWorldEntity(id);

        entityRegistry.AddOrUpdate(packet.Item);

        // Have other players respawn the item inside the inventory.
        await context.SendToOthersAsync(new SpawnEntities(packet.Item, forceRespawn: true));
    }

    private void PreserveExistingInstalledBatteries(NitroxId id, InventoryItemEntity pickedUpItem)
    {
        Optional<Entity> existingEntity = entityRegistry.GetEntityById(id);
        if (existingEntity is not { HasValue: true })
        {
            return;
        }

        foreach (InstalledBatteryEntity existingBattery in existingEntity.Value.ChildEntities.OfType<InstalledBatteryEntity>())
        {
            bool alreadyIncluded = pickedUpItem.ChildEntities
                .OfType<InstalledBatteryEntity>()
                .Any(incomingBattery =>
                    incomingBattery.Id == existingBattery.Id ||
                    incomingBattery.ComponentIndex == existingBattery.ComponentIndex);

            if (!alreadyIncluded)
            {
                pickedUpItem.ChildEntities.Add(existingBattery);
            }
        }
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
