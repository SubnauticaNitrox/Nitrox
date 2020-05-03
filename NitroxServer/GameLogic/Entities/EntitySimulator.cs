using System;
using System.Collections.Generic;
using System.Linq;
using DTO = NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private const DTO.SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = DTO.SimulationLockType.TRANSIENT;

        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;
        private readonly HashSet<DTO.TechType> serverSpawnedSimulationWhiteList;
        private readonly SimulationOwnershipData simulationOwnershipData;

        public EntitySimulation(EntityManager entityManager, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, HashSet<DTO.TechType> serverSpawnedSimulationWhiteList)
        {
            this.entityManager = entityManager;
            this.simulationOwnershipData = simulationOwnershipData;
            this.playerManager = playerManager;
            this.serverSpawnedSimulationWhiteList = serverSpawnedSimulationWhiteList;
        }

        public List<DTO.SimulatedEntity> CalculateSimulationChangesFromCellSwitch(Player player, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            List<DTO.SimulatedEntity> ownershipChanges = new List<DTO.SimulatedEntity>();

            AssignLoadedCellEntitySimulation(player, added, ownershipChanges);

            List<Entity> revokedEntities = RevokeForCells(player, removed);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        public List<DTO.SimulatedEntity> CalculateSimulationChangesFromPlayerDisconnect(Player player)
        {
            List<DTO.SimulatedEntity> ownershipChanges = new List<DTO.SimulatedEntity>();

            List<Entity> revokedEntities = RevokeAll(player);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        public DTO.SimulatedEntity AssignNewEntityToPlayer(Entity entity, Player player)
        {
            if (simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                return new DTO.SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            }

            throw new Exception("New entity was already being simulated by someone else: " + entity.Id);
        }

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, List<DTO.SimulatedEntity> ownershipChanges)
        {
            List<Entity> entities = AssignForCells(player, addedCells);

            foreach (Entity entity in entities)
            {
                ownershipChanges.Add(new DTO.SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<Entity> entities, List<DTO.SimulatedEntity> ownershipChanges)
        {
            foreach (Entity entity in entities)
            {
                foreach (Player player in playerManager.GetConnectedPlayers())
                {
                    bool isOtherPlayer = player != oldPlayer;

                    if (isOtherPlayer && player.CanSee(entity) && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info("Player " + player.Name + " has taken over simulating " + entity.Id);
                        ownershipChanges.Add(new DTO.SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                        return;
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

        private List<Entity> RevokeAll(Player player)
        {
            List<DTO.NitroxId> revokedEntities = simulationOwnershipData.RevokeAllForOwner(player);

            return entityManager.GetEntities(revokedEntities);
        }
    }
}
