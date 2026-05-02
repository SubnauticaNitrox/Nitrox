using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntitySpawnedByClientProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
    : IAuthPacketProcessor<EntitySpawnedByClient>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, EntitySpawnedByClient packet)
    {
        Entity entity = packet.Entity;

        // If the entity already exists in the registry, it is fine to update.  This is a normal case as the player
        // may have an item in their inventory (that the registry knows about) then wants to spawn it into the world.
        entityRegistry.AddOrUpdate(entity);

        SimulatedEntity simulatedEntity = null;
        if (entity is WorldEntity worldEntity)
        {
            worldEntityManager.TrackEntityInTheWorld(worldEntity);
        }

        if (packet.RequireSimulation && entitySimulation.TryAssignEntityToPlayer(entity, context.Sender, true, out simulatedEntity))
        {
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            await context.SendToAllAsync(ownershipChangePacket);
        }

        SpawnEntities spawnEntities = new(entity, simulatedEntity, packet.RequireRespawn);
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            bool isOtherPlayer = player != context.Sender;
            if (isOtherPlayer && player.CanSee(entity))
            {
                await context.SendAsync(spawnEntities, player.SessionId);
            }
        }
    }
}
