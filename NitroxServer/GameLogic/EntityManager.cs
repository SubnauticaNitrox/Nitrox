using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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
            foreach (Entity entity in entities)
            {
                entitiesByGuid.Add(entity.Guid, entity);
            }
        }

        private List<Entity> ListForCell(AbsoluteEntityCell absoluteEntityCell)
        {
            PrepareBatch(absoluteEntityCell.BatchId);

            List<Entity> result;
            if (!entitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
            {
                result = entitiesByAbsoluteCell[absoluteEntityCell] = new List<Entity>();
            }

            return result;
        }

        private void PrepareBatch(Int3 batchId)
        {
            Optional<IDictionary<AbsoluteEntityCell, List<Entity>>> opNewEntities = entitySpawner.SpawnUnloadedEntitiesForBatch(batchId);

            if (opNewEntities.IsPresent())
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
                lock (entitiesByAbsoluteCell)
                {
                    List<Entity> oldList;
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
                        foreach (Entity entity in cellEntities)
                        {
                            if (cell.Level <= entity.Level)
                            {
                                entities.Add(entity);
                            }
                        }
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
                        foreach (Entity entity in entities)
                        {
                            if (cell.Level <= entity.Level && simulationOwnership.TryToAcquire(entity.Guid, player))
                            {
                                assignedEntities.Add(entity);
                            }
                        }
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
                        foreach (Entity entity in entities)
                        {
                            if (entity.Level <= cell.Level && simulationOwnership.RevokeIfOwner(entity.Guid, player))
                            {
                                revokedEntities.Add(entity);
                            }
                        }
                    }
                }
            }

            return revokedEntities;
        }
    }
}
