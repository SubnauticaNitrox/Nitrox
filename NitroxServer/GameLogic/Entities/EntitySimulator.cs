using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;

        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly PlayerManager playerManager;
        private readonly HashSet<NitroxTechType> serverSpawnedSimulationWhiteList;
        private readonly SimulationOwnershipData simulationOwnershipData;

        public EntitySimulation(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, HashSet<NitroxTechType> serverSpawnedSimulationWhiteList)
        {
            this.entityRegistry = entityRegistry;
            this.worldEntityManager = worldEntityManager;
            this.simulationOwnershipData = simulationOwnershipData;
            this.playerManager = playerManager;
            this.serverSpawnedSimulationWhiteList = serverSpawnedSimulationWhiteList;
        }

        public void BroadcastSimulationChangesForPlayerCells(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<SimulatedEntity> ownershipChanges = new();

            IEnumerable<NitroxInt3> addedBatches = added.Select(abs => abs.BatchId).Distinct();
            AddBatches(player, addedBatches, ownershipChanges);

            IEnumerable<NitroxInt3> removedBatches = removed.Select(abs => abs.BatchId).Distinct();
            RemoveBatches(player, removedBatches, ownershipChanges);

            BroadcastSimulationChanges(ownershipChanges);
        }

        public void BroadcastSimulationChangesForBatchAddition(Player player, NitroxInt3 batchId)
        {
            List<SimulatedEntity> ownershipChanges = new();
            AddBatches(player, new[] { batchId }, ownershipChanges);
            BroadcastSimulationChanges(ownershipChanges);
        }

        private void RemoveBatches(Player player, IEnumerable<NitroxInt3> batches, List<SimulatedEntity> ownershipChanges)
        {
            foreach (NitroxInt3 removedBatch in batches)
            {
                List<WorldEntity> entities = worldEntityManager.GetEntities(removedBatch);
                IEnumerable<WorldEntity> revokedEntities = entities.Where(entity => !player.CanSee(entity) && simulationOwnershipData.RevokeIfOwner(entity.Id, player));
                AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);
            }
        }

        private void AddBatches(Player player, IEnumerable<NitroxInt3> batches, List<SimulatedEntity> ownershipChanges)
        {
            foreach (NitroxInt3 addedBatch in batches)
            {
                // It is possible that we process a CellVisibility request before this batch is actually loaded.
                // The batch can be skipped because it will again try to update simulated entities after loading.
                if (worldEntityManager.IsBatchSpawned(addedBatch))
                {
                    List<WorldEntity> entities = worldEntityManager.GetEntities(addedBatch);
                    List<WorldEntity> addedEntities = FilterSimulatableEntities(player, entities);

                    foreach (WorldEntity entity in addedEntities)
                    {
                        ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                    }
                }
            }
        }

        private void BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
        {
            if (ownershipChanges.Count > 0)
            {
                // TODO: This should be moved to `SimulationOwnership`
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }
        }

        public List<SimulatedEntity> CalculateSimulationChangesFromPlayerDisconnect(Player player)
        {
            List<SimulatedEntity> ownershipChanges = new List<SimulatedEntity>();

            List<NitroxId> revokedEntities = RevokeAll(player);
            AssignSimulationsToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        public SimulatedEntity AssignNewEntityToPlayer(Entity entity, Player player)
        {
            if (simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                return new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            }

            throw new Exception($"New entity was already being simulated by someone else: {entity.Id}");
        }

        public IEnumerable<NitroxId> AssignGlobalRootEntities(Player player)
        {
            List<WorldEntity> globalRootEntities = worldEntityManager.GetGlobalRootEntities();
            IEnumerable<WorldEntity> entities = globalRootEntities.Where(entity => simulationOwnershipData.TryToAcquire(entity.Id, player, SimulationLockType.TRANSIENT));
            foreach (WorldEntity entity in entities)
            {
                yield return entity.Id;
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, IEnumerable<WorldEntity> entities, List<SimulatedEntity> ownershipChanges)
        {
            foreach (WorldEntity entity in entities)
            {
                foreach (Player player in playerManager.GetConnectedPlayers())
                {
                    bool isOtherPlayer = player != oldPlayer;

                    if (isOtherPlayer && player.CanSee(entity) && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info($"Player {player.Name} has taken over simulating {entity.Id}");

                        bool isSpawnedByServerAndWhitelisted = entity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(entity.TechType);
                        bool doesEntityMove = isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer;

                        ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                        break;
                    }
                }
            }
        }

        private void AssignSimulationsToNewPlayers(Player oldPlayer, List<NitroxId> entities, List<SimulatedEntity> ownershipChanges)
        {
            foreach (NitroxId id in entities)
            {
                foreach (Player player in playerManager.GetConnectedPlayers())
                {
                    bool isOtherPlayer = player != oldPlayer;

                    Optional<Entity> opEntity = entityRegistry.GetEntityById(id);
                    Entity entity = opEntity.OrNull();

                    if (isOtherPlayer && (entity == null || player.CanSee(entity)) && simulationOwnershipData.TryToAcquire(id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info($"Player {player.Name} has taken over simulating {id}");
                        ownershipChanges.Add(new SimulatedEntity(id, player.Id, entity != null, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                        break;
                    }
                }
            }
        }

        private List<WorldEntity> FilterSimulatableEntities(Player player, List<WorldEntity> entities)
        {
            return entities.Where(entity => {
                    bool isSpawnedByServerAndWhitelisted = entity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(entity.TechType);
                    bool isEligibleForSimulation = player.HasCellLoaded(entity.AbsoluteEntityCell) && (isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer);
                    return isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                }).ToList();
        }

        private List<NitroxId> RevokeAll(Player player)
        {
            List<NitroxId> revokedEntities = simulationOwnershipData.RevokeAllForOwner(player);

            return revokedEntities;
        }

        public void EntityDestroyed(NitroxId id)
        {
            simulationOwnershipData.RevokeOwnerOfId(id);
        }
    }
}
