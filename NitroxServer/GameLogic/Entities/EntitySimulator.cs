using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;

        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;
        private readonly HashSet<NitroxTechType> serverSpawnedSimulationWhiteList;
        private readonly SimulationOwnershipData simulationOwnershipData;

        public EntitySimulation(EntityManager entityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, HashSet<NitroxTechType> serverSpawnedSimulationWhiteList)
        {
            this.entityManager = entityManager;
            this.simulationOwnershipData = simulationOwnershipData;
            this.playerManager = playerManager;
            this.serverSpawnedSimulationWhiteList = serverSpawnedSimulationWhiteList;
        }

        public List<SimulatedEntity> CalculateSimulationChangesFromCellSwitch(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<SimulatedEntity> ownershipChanges = new List<SimulatedEntity>();

            AssignLoadedCellEntitySimulation(player, added, ownershipChanges);

            List<Entity> revokedEntities = RevokeForCells(player, removed);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
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

            throw new Exception("New entity was already being simulated by someone else: " + entity.Id);
        }

        public IEnumerable<NitroxId> AssignGlobalRootEntities(Player player)
        {
            List<Entity> globalRootEntities = entityManager.GetGlobalRootEntities();
            IEnumerable<Entity> entities = globalRootEntities.Where(entity => simulationOwnershipData.TryToAcquire(entity.Id, player, SimulationLockType.TRANSIENT));
            foreach (Entity entity in entities)
            {
                yield return entity.Id;
            }
        }

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, List<SimulatedEntity> ownershipChanges)
        {
            List<Entity> entities = AssignForCells(player, addedCells);

            foreach (Entity entity in entities)
            {
                ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<Entity> entities, List<SimulatedEntity> ownershipChanges)
        {
            foreach (Entity entity in entities)
            {
                foreach (Player player in playerManager.GetConnectedPlayers())
                {
                    bool isOtherPlayer = player != oldPlayer;

                    if (isOtherPlayer && player.CanSee(entity) && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info("Player " + player.Name + " has taken over simulating " + entity.Id);
                        ownershipChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
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
                    Optional<Entity> opEntity = entityManager.GetEntityById(id);
                    Entity entity = opEntity.OrNull();
                    if (isOtherPlayer && (entity == null || player.CanSee(entity)) && simulationOwnershipData.TryToAcquire(id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info("Player " + player.Name + " has taken over simulating " + id);
                        ownershipChanges.Add(new SimulatedEntity(id, player.Id, entity != null, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                        break;
                    }
                }
            }
        }

        private List<Entity> AssignForCells(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities = entityManager.GetEntities(cell);
                assignedEntities.AddRange(
                    entities.Where(entity =>
                    {
                        bool isSpawnedByServerAndWhitelisted = entity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(entity.TechType);
                        bool isEligibleForSimulation = isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer;
                        return cell.Level <= entity.Level && isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                    }));
            }

            return assignedEntities;
        }

        private List<Entity> RevokeForCells(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();
            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities = entityManager.GetEntities(cell);
                revokedEntities.AddRange(entities.Where(entity => entity.Level <= cell.Level && simulationOwnershipData.RevokeIfOwner(entity.Id, player)));
            }

            return revokedEntities;
        }

        private List<NitroxId> RevokeAll(Player player)
        {
            List<NitroxId> revokedEntities = simulationOwnershipData.RevokeAllForOwner(player);

            return revokedEntities;
        }
    }
}
