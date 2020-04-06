using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityManager
    {
        private readonly Dictionary<NitroxId, Entity> entitiesById;

        // Phasing entities can disappear if you go out of range. 
        private readonly Dictionary<AbsoluteEntityCell, List<Entity>> phasingEntitiesByAbsoluteCell;

        // Global root entities that are always visible.
        private readonly Dictionary<NitroxId, Entity> globalRootEntitiesById;
        
        private readonly BatchEntitySpawner batchEntitySpawner;

        public EntityManager(List<Entity> entities, BatchEntitySpawner batchEntitySpawner)
        {
            entitiesById = entities.ToDictionary(entity => entity.Id);

            globalRootEntitiesById = entities.FindAll(entity => entity.ExistsInGlobalRoot)
                                             .ToDictionary(entity => entity.Id);

            phasingEntitiesByAbsoluteCell = entities.FindAll(entity => !entity.ExistsInGlobalRoot)
                                                    .GroupBy(entity => entity.AbsoluteEntityCell)
                                                    .ToDictionary(group => group.Key, group => group.ToList());

            this.batchEntitySpawner = batchEntitySpawner;
        }

        public List<Entity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            LoadUnspawnedEntities(cells);

            List<Entity> entities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<Entity> cellEntities = GetEntities(cell);                
                entities.AddRange(cellEntities.Where(entity => cell.Level <= entity.Level));                                
            }

            return entities;
        }

        public Optional<Entity> GetEntityById(NitroxId id)
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
            }

            return Optional.OfNullable(entity);
        }

        public List<Entity> GetAllEntities()
        {
            lock (entitiesById)
            {
                return new List<Entity>(entitiesById.Values);
            }
        }

        public List<Entity> GetGlobalRootEntities()
        {
            lock (globalRootEntitiesById)
            {
                return new List<Entity>(globalRootEntitiesById.Values);
            }
        }        

        public List<Entity> GetEntities(AbsoluteEntityCell absoluteEntityCell)
        {
            List<Entity> result;

            lock (phasingEntitiesByAbsoluteCell)
            {
                if (!phasingEntitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = phasingEntitiesByAbsoluteCell[absoluteEntityCell] = new List<Entity>();
                }
            }

            return result;
        }

        public List<Entity> GetEntities(List<NitroxId> ids)
        {
            lock (entitiesById)
            {
                return entitiesById.Join(ids, 
                                         entity => entity.Value.Id, 
                                         id => id, 
                                         (entity, id) => entity.Value)
                                   .ToList();
            }
        }

        public Optional<AbsoluteEntityCell> UpdateEntityPosition(NitroxId id, Vector3 position, Quaternion rotation)
        {
            Optional<Entity> opEntity = GetEntityById(id);
            if (!opEntity.HasValue)
            {
                Log.Debug("Could not update entity position because it was not found (maybe it was recently picked up)");
                return Optional.Empty;
            }

            Entity entity = opEntity.Value;
            AbsoluteEntityCell oldCell = entity.AbsoluteEntityCell;

            entity.Transform.Position = position;
            entity.Transform.Rotation = rotation;

            AbsoluteEntityCell newCell = entity.AbsoluteEntityCell;
            if (oldCell != newCell)
            {
                EntitySwitchedCells(entity, oldCell, newCell);
            }

            return Optional.Of(newCell);
        }

        public void RegisterNewEntity(Entity entity)
        {
            lock (entitiesById)
            {
                entitiesById.Add(entity.Id, entity);
            }

            if (entity.ExistsInGlobalRoot)
            {
                lock (globalRootEntitiesById)
                {
                    if (!globalRootEntitiesById.ContainsKey(entity.Id))
                    {
                        globalRootEntitiesById.Add(entity.Id, entity);
                    }
                    else
                    {
                        Log.Info("Entity Already Exists for Id: " + entity.Id + " Item: " + entity.TechType);
                    }
                }
            }
            else
            {
                lock (phasingEntitiesByAbsoluteCell)
                {
                    List<Entity> phasingEntitiesInCell = null;

                    if (!phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out phasingEntitiesInCell))
                    {
                        phasingEntitiesInCell = phasingEntitiesByAbsoluteCell[entity.AbsoluteEntityCell] = new List<Entity>();
                    }

                    phasingEntitiesInCell.Add(entity);
                }
            }
        }

        public void PickUpEntity(NitroxId id)
        {
            Entity entity;
            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
                entitiesById.Remove(id);
            }

            if (entity != null)
            {
                if (entity.ExistsInGlobalRoot)
                {
                    lock (globalRootEntitiesById)
                    {
                        globalRootEntitiesById.Remove(id);
                    }
                }
                else
                {
                    lock (phasingEntitiesByAbsoluteCell)
                    {
                        List<Entity> entities;

                        if (phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out entities))
                        {
                            entities.Remove(entity);
                        }
                    }
                }
            }
        }

        private void LoadUnspawnedEntities(AbsoluteEntityCell[] cells)
        {
            IEnumerable<Int3> distinctBatchIds = cells.Select(cell => cell.BatchId).Distinct();

            foreach(Int3 batchId in distinctBatchIds)
            {
                List<Entity> spawnedEntities = batchEntitySpawner.LoadUnspawnedEntities(batchId);

                lock (entitiesById)
                {
                    lock (phasingEntitiesByAbsoluteCell)
                    {
                        foreach (Entity entity in spawnedEntities)
                        {
                            if (entity.ParentId != null)
                            {
                                Optional<Entity> opEnt = GetEntityById(entity.ParentId);

                                if (opEnt.HasValue)
                                {
                                    entity.Transform.SetParent(opEnt.Value.Transform);
                                }
                                else
                                {
                                    Log.Error("Parent not Found! Are you sure it exists? " + entity.ParentId);
                                }
                            }

                            List<Entity> entitiesInCell = GetEntities(entity.AbsoluteEntityCell);
                            entitiesInCell.Add(entity);

                            entitiesById.Add(entity.Id, entity);
                        }
                    }
                }
            }
        }

        private void EntitySwitchedCells(Entity entity, AbsoluteEntityCell oldCell, AbsoluteEntityCell newCell)
        {
            if (entity.ExistsInGlobalRoot)
            {
                // We don't care what cell a global root entity resides in.  Only phasing entities.
                return;
            }

            lock (phasingEntitiesByAbsoluteCell)
            {
                List<Entity> oldList = GetEntities(oldCell);
                oldList.Remove(entity);

                List<Entity> newList = GetEntities(newCell);
                newList.Add(entity);
            }
        }
    }
}
