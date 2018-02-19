using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EntityManager
    {
        private readonly EntitySpawner entitySpawner;
        private readonly SimulationOwnership simulationOwnership;

        public EntityManager(EntitySpawner entitySpawner, SimulationOwnership simulationOwnership)
        {
            this.entitySpawner = entitySpawner;
            this.simulationOwnership = simulationOwnership;
        }

        public Entity UpdateEntityPosition(string guid, Vector3 position, Quaternion rotation)
        {
            Entity entity = GetEntityByGuid(guid);
            AbsoluteEntityCell oldCell = entity.AbsoluteEntityCell;
            AbsoluteEntityCell newCell = new AbsoluteEntityCell(position, entity.Level);

            if (oldCell != newCell)
            {
                List<Entity> oldList;
                if (entitySpawner.GetEntities(oldCell, out oldList))
                {
                    lock (oldList)
                    {
                        // Validate.IsTrue?
                        oldList.Remove(entity);
                    }
                }

                List<Entity> newList = entitySpawner.GetEntities(newCell);

                lock (newList)
                {
                    newList.Add(entity);
                }
            }

            entity.Position = position;
            entity.Rotation = rotation;

            return entity;
        }

        public Entity GetEntityByGuid(string guid)
        {
            Entity entity = null;

            Validate.IsTrue(entitySpawner.TryGetEntityByGuid(guid, out entity));

            Validate.NotNull(entity);

            return entity;
        }

        public List<Entity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            List<Entity> entities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<Entity> cellEntities;

                if (entitySpawner.GetEntities(cell, out cellEntities))
                {
                    lock (cellEntities)
                    {
                        entities.AddRange(cellEntities.Where(entity => cell.Level <= entity.Level));
                    }
                }
            }

            return entities;
        }

        public List<Entity> AssignEntitySimulation(Player player, AbsoluteEntityCell[] added)
        {
            List<Entity> assignedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in added)
            {
                List<Entity> entities;

                if (entitySpawner.GetEntities(cell, out entities))
                {
                    lock (entities)
                    {
                        assignedEntities.AddRange(
                            entities.Where(entity => cell.Level <= entity.Level && simulationOwnership.TryToAcquire(entity.Guid, player)));
                    }
                }
            }

            return assignedEntities;
        }

        public List<Entity> RevokeEntitySimulationFor(Player player, AbsoluteEntityCell[] removed)
        {
            List<Entity> revokedEntities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in removed)
            {
                List<Entity> entities;

                if (entitySpawner.GetEntities(cell, out entities))
                {
                    lock (entities)
                    {
                        revokedEntities.AddRange(
                            entities.Where(entity => entity.Level <= cell.Level && simulationOwnership.RevokeIfOwner(entity.Guid, player)));
                    }
                }
            }

            return revokedEntities;
        }
    }
}
