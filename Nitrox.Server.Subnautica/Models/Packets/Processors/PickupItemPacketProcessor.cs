using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PickupItemPacketProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData)
    : AuthenticatedPacketProcessor<PickupItem>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(PickupItem packet, Player player)
    {
        NitroxId id = packet.Item.Id;
        if (simulationOwnershipData.RevokeOwnerOfId(id))
        {
            ushort serverId = ushort.MaxValue;
            SimulationOwnershipChange simulationOwnershipChange = new(id, serverId, SimulationLockType.TRANSIENT);
            packetSender.SendPacketToOthersAsync(simulationOwnershipChange, player.SessionId);
        }

        StopTrackingExistingWorldEntity(id);

        entityRegistry.AddOrUpdate(packet.Item);

        // Have other players respawn the item inside the inventory.
        packetSender.SendPacketToOthersAsync(new SpawnEntities(packet.Item, forceRespawn: true), player.SessionId);
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
