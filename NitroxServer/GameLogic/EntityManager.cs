using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Spawning;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class EntityManager
    {
        private readonly Dictionary<AbsoluteEntityCell, List<Entity>> entitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
        private readonly Dictionary<string, Entity> entitiesByGuid = new Dictionary<string, Entity>();

        private readonly EntitySpawner entitySpawner;
        private readonly SimulationOwnership simulationOwnership;

        public EntityManager(EntitySpawner entitySpawner, SimulationOwnership simulationOwnership)
        {
            this.entitySpawner = entitySpawner;
            this.simulationOwnership = simulationOwnership;
        }

        private void RegisterEntities(IEnumerable<Entity> entities)
        {
            lock (entitiesByGuid)
            {
                foreach (Entity entity in entities)
                {
                    entitiesByGuid.Add(entity.Guid, entity);
                }
            }
        }

        private List<Entity> ListForCell(AbsoluteEntityCell absoluteEntityCell)
        {
            PrepareBatch(absoluteEntityCell.BatchId);

            List<Entity> result;

            lock (entitiesByAbsoluteCell)
            {
                if (!entitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = entitiesByAbsoluteCell[absoluteEntityCell] = new List<Entity>();
                }
            }

            return result;
        }

        private void PrepareBatch(Int3 batchId)
        {
            Optional<IDictionary<AbsoluteEntityCell, List<Entity>>> opNewEntities = entitySpawner.SpawnUnloadedEntitiesForBatch(batchId);

            if (opNewEntities.IsPresent())
            {

                lock (entitiesByAbsoluteCell)
                {
                    // Batch cell was not loaded yet. Process outstanding entity spawns:
                    foreach (KeyValuePair<AbsoluteEntityCell, List<Entity>> kvp in opNewEntities.Get())
                    {
                        List<Entity> entitiesInCell = ListForCell(kvp.Key);

                        entitiesInCell.AddRange(kvp.Value);

                        RegisterEntities(kvp.Value);
                    }
                }
            }

        }

        private List<Entity> GetEntities(AbsoluteEntityCell absoluteEntityCell)
        {
            PrepareBatch(absoluteEntityCell.BatchId);

            return ListForCell(absoluteEntityCell);
        }

        private bool GetEntities(AbsoluteEntityCell absoluteEntityCell, out List<Entity> result)
        {
            PrepareBatch(absoluteEntityCell.BatchId);

            return entitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result);
        }

        public Entity UpdateEntityPosition(string guid, Vector3 position, Quaternion rotation)
        {
            Entity entity = GetEntityByGuid(guid);
            AbsoluteEntityCell oldCell = entity.AbsoluteEntityCell;
            AbsoluteEntityCell newCell = new AbsoluteEntityCell(position, entity.Level);

            if (oldCell != newCell)
            {
                List<Entity> oldList;

                lock (entitiesByAbsoluteCell)
                {
                    if (GetEntities(oldCell, out oldList))
                    {
                        // Validate.IsTrue?
                        oldList.Remove(entity);
                    }

                    List<Entity> newList = GetEntities(newCell);
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

            lock (entitiesByGuid)
            {
                entitiesByGuid.TryGetValue(guid, out entity);
            }

            Validate.NotNull(entity);

            return entity;
        }

        public List<Entity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            List<Entity> entities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<Entity> cellEntities;

                lock (entitiesByAbsoluteCell)
                {
                    if (GetEntities(cell, out cellEntities))
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

                lock (entitiesByAbsoluteCell)
                {
                    if (GetEntities(cell, out entities))
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

                lock (entitiesByAbsoluteCell)
                {
                    if (GetEntities(cell, out entities))
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
