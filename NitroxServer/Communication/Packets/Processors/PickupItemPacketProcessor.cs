using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PickupItemPacketProcessor : AuthenticatedPacketProcessor<PickupItem>
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly PlayerManager playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public PickupItemPacketProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.playerManager = playerManager;
        this.simulationOwnershipData = simulationOwnershipData;
    }

    public override void Process(PickupItem packet, Player player)
    {
        if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
        {
            ushort serverId = ushort.MaxValue;
            SimulationOwnershipChange simulationOwnershipChange = new(packet.Id, serverId, SimulationLockType.TRANSIENT);
            playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
        }

        StopTrackingExistingWorldEntity(packet.Id);

        entityRegistry.AddOrUpdate(packet.Item);

        // Have other players respawn the item inside the inventory.
        playerManager.SendPacketToOtherPlayers(new SpawnEntities(packet.Item, true), player);
    }

    private void StopTrackingExistingWorldEntity(NitroxId id)
    {
        Optional<Entity> entity = entityRegistry.GetEntityById(id);

        if (entity.HasValue && entity.Value is WorldEntity worldEntity)
        {
            // Do not track this entity in the open world anymore.
            worldEntityManager.StopTrackingEntity(worldEntity);
        }
    }
}
