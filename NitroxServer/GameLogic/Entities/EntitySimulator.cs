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

        public void BroadcastSimulationChangesForCellUpdates(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<SimulatedEntity> ownershipChanges = new();

            AddCells(player, added, ownershipChanges);
            RemoveCells(player, removed, ownershipChanges);

            BroadcastSimulationChanges(ownershipChanges);
        }

        public void BroadcastSimulationChangesForBatchAddition(Player player, NitroxInt3 batchId)
        {
            List<WorldEntity> entities = worldEntityManager.GetEntities(batchId);
            List<WorldEntity> addedEntities = FilterSimulatableEntities(player, entities);

            List<SimulatedEntity> ownershipChanges = new();

            foreach (WorldEntity entity in addedEntities)
            {
                ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
            }

            BroadcastSimulationChanges(ownershipChanges);
        }

        private void RemoveCells(Player player, AbsoluteEntityCell[] removed, List<SimulatedEntity> ownershipChanges)
        {
            foreach (AbsoluteEntityCell cell in removed)
            {
                List<WorldEntity> entities = worldEntityManager.GetEntities(cell);
                IEnumerable<WorldEntity> revokedEntities = entities.Where(entity => !player.CanSee(entity) && simulationOwnershipData.RevokeIfOwner(entity.Id, player));
                ReassignEntitiesToOtherPlayers(player, revokedEntities, ownershipChanges);
            }
        }

        private void AddCells(Player player, AbsoluteEntityCell[] added, List<SimulatedEntity> ownershipChanges)
        {
            foreach (AbsoluteEntityCell cell in added)
            {
                List<WorldEntity> entities = worldEntityManager.GetEntities(cell);
                List<WorldEntity> addedEntities = FilterSimulatableEntities(player, entities);

                foreach (WorldEntity entity in addedEntities)
                {
                    ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
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

            List<NitroxId> revokedEntities = simulationOwnershipData.RevokeAllForOwner(player);
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
            List<WorldEntity> globalRootEntities = new(worldEntityManager.GetGlobalRootEntities());
            IEnumerable<WorldEntity> entities = globalRootEntities.Where(entity => simulationOwnershipData.TryToAcquire(entity.Id, player, SimulationLockType.TRANSIENT));
            foreach (WorldEntity entity in entities)
            {
                yield return entity.Id;
            }
        }

        private void ReassignEntitiesToOtherPlayers(Player oldPlayer, IEnumerable<WorldEntity> entities, List<SimulatedEntity> ownershipChanges)
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
                    bool isEligibleForSimulation = player.CanSee(entity) && (isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer);
                    return isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                }).ToList();
        }

        public void EntityDestroyed(NitroxId id)
        {
            simulationOwnershipData.RevokeOwnerOfId(id);
        }
    }
}
