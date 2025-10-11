using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class EntitySpawnedByClientProcessor : AuthenticatedPacketProcessor<EntitySpawnedByClient>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;

        public EntitySpawnedByClientProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
            this.worldEntityManager = worldEntityManager;
            this.entitySimulation = entitySimulation;
        }

        public override void Process(EntitySpawnedByClient packet, Player playerWhoSpawned)
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

            if (packet.RequireSimulation && entitySimulation.TryAssignEntityToPlayer(entity, playerWhoSpawned, true, out simulatedEntity))
            {
                SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
                playerManager.SendPacketToAllPlayers(ownershipChangePacket);
            }

            SpawnEntities spawnEntities = new(entity, simulatedEntity, packet.RequireRespawn);
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != playerWhoSpawned;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.SendPacket(spawnEntities);
                }
            }
        }
    }
}
