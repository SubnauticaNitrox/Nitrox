﻿using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Entities
{
    public class EntitySimulation
    {
        private static readonly SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;

        private readonly EntityData entityData;
        private readonly SimulationOwnershipData simulationOwnershipData;
        private readonly PlayerManager playerManager;
        private readonly HashSet<TechType> serverSpawnedSimulationWhiteList;

        public EntitySimulation(EntityData entityData, SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, HashSet<TechType> serverSpawnedSimulationWhiteList)
        {
            this.entityData = entityData;
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

            List<Entity> revokedEntities = RevokeAll(player);
            AssignEntitiesToNewPlayers(player, revokedEntities, ownershipChanges);

            return ownershipChanges;
        }

        private void AssignLoadedCellEntitySimulation(Player player, AbsoluteEntityCell[] addedCells, List<SimulatedEntity> ownershipChanges)
        {
            List<Entity> entities = AssignForCells(player, addedCells);

            foreach (Entity entity in entities)
            {
                ownershipChanges.Add(new SimulatedEntity(entity.Guid, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                if (entity.ChildEntitiesByGuid.Count > 0)
                {
                    AssignLoadedCellChildEntitySimulation(player, entity, ownershipChanges);
                }
            }
        }

        private void AssignLoadedCellChildEntitySimulation(Player player, Entity parent, List<SimulatedEntity> ownershipChanges)
        {
            foreach (Entity childEntity in parent.ChildEntitiesByGuid.Values)
            {
                ownershipChanges.Add(new SimulatedEntity(childEntity.Guid, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                if (childEntity.ChildEntitiesByGuid.Count > 0)
                {
                    AssignLoadedCellChildEntitySimulation(player, childEntity, ownershipChanges);
                }
            }
        }

        private void AssignEntitiesToNewPlayers(Player oldPlayer, List<Entity> entities, List<SimulatedEntity> ownershipChanges)
        {
            foreach (Entity entity in entities)
            {
                AbsoluteEntityCell entityCell = entity.AbsoluteEntityCell;

                foreach (Player player in playerManager.GetPlayers())
                {
                    bool isOtherPlayer = (player != oldPlayer);

                    if (isOtherPlayer && player.CanSee(entity) && simulationOwnershipData.TryToAcquire(entity.Guid, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        Log.Info("Player " + player.Name + " has taken over simulating " + entity.Guid);
                        ownershipChanges.Add(new SimulatedEntity(entity.Guid, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
                        return;
                    }
                }
            }
        }
        
        public SimulatedEntity AssignNewEntityToPlayer(Entity entity, Player player)
        {
            if(simulationOwnershipData.TryToAcquire(entity.Guid, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
            {
                return new SimulatedEntity(entity.Guid, player.Id, true, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
            }

            throw new System.Exception("New entity was already being simulated by someone else: " + entity.Guid);
        }

        private List<Entity> AssignForCells(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities = entityData.GetEntities(cell);

                foreach (Entity entity in entities)
                {
                    if (cell.Level <= entity.Level && simulationOwnershipData.TryToAcquire(entity.Guid, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
                    {
                        assignedEntities.Add(entity);
                        assignedEntities.AddRange(
                        entity.ChildEntitiesByGuid.Values.Where(childEntity => cell.Level <= childEntity.Level &&
                                                    ((childEntity.SpawnedByServer && serverSpawnedSimulationWhiteList.Contains(childEntity.TechType)) || !childEntity.SpawnedByServer) &&
                                                    simulationOwnershipData.TryToAcquire(childEntity.Guid, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE)));
                    }
                }
            }

            return assignedEntities;
        }

        private List<Entity> RevokeForCells(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities = entityData.GetEntities(cell);
                
                foreach(Entity entity in entities)
                {
                    if (entity.Level <= cell.Level && simulationOwnershipData.RevokeIfOwner(entity.Guid, player))
                    {
                        revokedEntities.Add(entity);
                        revokedEntities.AddRange(entity.ChildEntitiesByGuid.Values.Where(childEntity => childEntity.Level <= cell.Level && simulationOwnershipData.RevokeIfOwner(childEntity.Guid, player)));
                    }
                }                    
            }

            return revokedEntities;
        }

        private List<Entity> RevokeAll(Player player)
        {
            List<string> RevokedGuids = simulationOwnershipData.RevokeAllForOwner(player);

            return entityData.GetEntitiesByGuids(RevokedGuids);
        }
    }
}
