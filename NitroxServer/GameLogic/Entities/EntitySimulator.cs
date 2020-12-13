using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Logger;

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

        public Dictionary<NitroxInt3, List<SimulatedEntity>> CalculateSimulationChangesFromCellSwitch(Player player, CellChanges cellChanges)
        {
            Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges = new Dictionary<NitroxInt3, List<SimulatedEntity>>();

            AssignLoadedCellEntitySimulation(player, cellChanges.Added.ToArray(), ownershipChanges);

            List<Entity> revokedEntities = RevokeForCells(player, cellChanges.Removed.ToArray());
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        public Dictionary<NitroxInt3, List<SimulatedEntity>> CalculateSimulationChangesFromPlayerDisconnect(Player player)
        {
            Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges = new Dictionary<NitroxInt3, List<SimulatedEntity>>();

            List<Entity> revokedEntities = RevokeAll(player);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

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

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges)
        {
            List<Entity> entities = AssignForCells(player, addedCells);

            foreach (Entity entity in entities)
            {
                if (!ownershipChanges.TryGetValue(entity.AbsoluteEntityCell.BatchId, out List<SimulatedEntity> simChanges))
                {
                    simChanges = new List<SimulatedEntity>();
                    ownershipChanges.Add(entity.AbsoluteEntityCell.BatchId, simChanges);
                }
                simChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<Entity> entities, Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges)
        {
            foreach (Entity entity in entities)
            {
                foreach (Player player in playerManager.GetConnectedPlayers())
                {
                    bool isOtherPlayer = player != oldPlayer;

                    if (isOtherPlayer && player.CanSee(entity) && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info("Player " + player.Name + " has taken over simulating " + entity.Id);
                        if (!ownershipChanges.TryGetValue(entity.AbsoluteEntityCell.BatchId, out List<SimulatedEntity> simChanges))
                        {
                            simChanges = new List<SimulatedEntity>();
                            ownershipChanges.Add(entity.AbsoluteEntityCell.BatchId, simChanges);
                        }
                        simChanges.Add(new SimulatedEntity(entity.Id, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
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
                List<Entity> entities = entityManager.GetEntities(cell.BatchId);
                assignedEntities.AddRange(
                    entities.Where(entity =>
                    {
                        bool isSpawnedByServerAndWhitelisted = entity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(entity.TechType);
                        bool isEligibleForSimulation = isSpawnedByServerAndWhitelisted || !entity.SpawnedByServer;
                        return simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
                    }));
            }

            return assignedEntities;
        }

        private List<Entity> RevokeForCells(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();
            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities = entityManager.GetEntities(cell.BatchId);
                revokedEntities.AddRange(entities.Where(entity => simulationOwnershipData.RevokeIfOwner(entity.Id, player)));
            }

            return revokedEntities;
        }

        private List<Entity> RevokeAll(Player player)
        {
            List<NitroxId> revokedEntities = simulationOwnershipData.RevokeAllForOwner(player);

            return entityManager.GetEntities(revokedEntities);
        }
    }
}
