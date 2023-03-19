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

        public void CalculateSimulationChangesFromCellSwitch(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<SimulatedEntity> ownershipChanges = new List<SimulatedEntity>();
            AddCells(player, added, ownershipChanges);
            RemoveCells(player, removed, ownershipChanges);

            BroadcastSimulationChanges(ownershipChanges);
        }

        private void RemoveCells(Player player, AbsoluteEntityCell[] removed, List<SimulatedEntity> ownershipChanges)
        {
            foreach (NitroxInt3 removedBatch in removed.Select(abs => abs.BatchId).Distinct())
            {
                AbsoluteEntityCell[] batchCells = removed.Where(add => add.BatchId == removedBatch).ToArray();
                if (worldEntityManager.IsBatchSpawned(removedBatch))
                {
                    List<WorldEntity> entities = worldEntityManager.GetEntities(removedBatch);
                    List<WorldEntity> revokedEntities = RevokeForCells(player, removed, entities);
                    AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);
                }
                else
                {
                    worldEntityManager.OnBatchLoad += removeEvent;
                    void removeEvent(NitroxInt3 eventBatch)
                    {
                        if (removedBatch == eventBatch)
                        {
                            List<WorldEntity> entities = worldEntityManager.GetEntities(removedBatch);
                            List<SimulatedEntity> localOwnershipChanges = new List<SimulatedEntity>();
                            List<WorldEntity> revokedEntities = RevokeForCells(player, removed, entities);
                            AssignEntitiesToNewPlayers(player, revokedEntities, localOwnershipChanges);
                            BroadcastSimulationChanges(localOwnershipChanges);
                            worldEntityManager.OnBatchLoad -= removeEvent;
                        }
                    };
                }
            }
        }

        private void AddCells(Player player, AbsoluteEntityCell[] added, List<SimulatedEntity> ownershipChanges)
        {
            foreach (NitroxInt3 addedBatch in added.Select(abs => abs.BatchId).Distinct())
            {
                AbsoluteEntityCell[] batchCells = added.Where(add => add.BatchId == addedBatch).ToArray();
                if (worldEntityManager.IsBatchSpawned(addedBatch))
                {
                    List<WorldEntity> entities = worldEntityManager.GetEntities(addedBatch);
                    AssignLoadedCellEntitySimulation(player, batchCells, ownershipChanges, entities);
                }
                else
                {
                    void addEvent(NitroxInt3 eventBatch)
                    {
                        if (addedBatch == eventBatch)
                        {
                            List<WorldEntity> entities = worldEntityManager.GetEntities(addedBatch);
                            List<SimulatedEntity> localOwnershipChanges = new List<SimulatedEntity>();
                            AssignLoadedCellEntitySimulation(player, batchCells, localOwnershipChanges, entities);
                            BroadcastSimulationChanges(localOwnershipChanges);
                            worldEntityManager.OnBatchLoad -= addEvent;
                        }
                    };

                    worldEntityManager.OnBatchLoad += addEvent;
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

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, List<SimulatedEntity> ownershipChanges, List<WorldEntity> entities)
        {
            List<Entity> addedEntities = AssignForCells(player, addedCells, entities);

            foreach (Entity entity in addedEntities)
            {
                ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<WorldEntity> entities, List<SimulatedEntity> ownershipChanges)
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

        private List<Entity> AssignForCells(Player player, AbsoluteEntityCell[] added, List<WorldEntity> entities)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                assignedEntities.AddRange(
                    entities.Where(entity =>
                    {
                        bool isSpawnedByServerAndWhitelisted = entity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(entity.TechType);
                        bool isEligibleForSimulation = entity.AbsoluteEntityCell.CellId == cell.CellId && (isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer);
                        return cell.Level <= entity.Level && isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                    }));
            }

            return assignedEntities;
        }

        private List<WorldEntity> RevokeForCells(Player player, AbsoluteEntityCell[] removed, List<WorldEntity> entities)
        {
            List<WorldEntity> revokedEntities = new List<WorldEntity>();
            foreach (AbsoluteEntityCell cell in removed)
            {
                revokedEntities.AddRange(entities.Where(entity => entity.Level <= cell.Level && entity.AbsoluteEntityCell == cell && simulationOwnershipData.RevokeIfOwner(entity.Id, player)));
            }

            return revokedEntities;
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
