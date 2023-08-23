using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.GameLogic.Entities
{
    public class WorldEntityManager
    {
        private readonly EntityRegistry entityRegistry;

        /// <summary>
        ///     Phasing entities can disappear if you go out of range.
        /// </summary>
        private readonly Dictionary<NitroxInt3, List<WorldEntity>> phasingEntitiesByBatchId;

        private readonly Dictionary<AbsoluteEntityCell, List<WorldEntity>> phasingEntitiesByCellId;

        /// <summary>
        ///     Global root entities that are always visible.
        /// </summary>
        private readonly Dictionary<NitroxId, GlobalRootEntity> globalRootEntitiesById;

        private readonly BatchEntitySpawner batchEntitySpawner;

        private readonly object worldEntitiesLock;
        private readonly object globalRootEntitiesLock;

        public WorldEntityManager(EntityRegistry entityRegistry, BatchEntitySpawner batchEntitySpawner)
        {
            List<WorldEntity> worldEntities = entityRegistry.GetEntities<WorldEntity>();

            globalRootEntitiesById = entityRegistry.GetEntities<GlobalRootEntity>().ToDictionary(entity => entity.Id);

            phasingEntitiesByBatchId = worldEntities.Where(entity => entity is not GlobalRootEntity)
                                                    .GroupBy(entity => entity.AbsoluteEntityCell.BatchId)
                                                    .ToDictionary(group => group.Key, group => group.ToList());

            phasingEntitiesByCellId = worldEntities.Where(entity => entity is not GlobalRootEntity)
                                                   .GroupBy(entity => entity.AbsoluteEntityCell)
                                                   .ToDictionary(group => group.Key, group => group.ToList());
            this.entityRegistry = entityRegistry;
            this.batchEntitySpawner = batchEntitySpawner;

            worldEntitiesLock = new();
            globalRootEntitiesLock = new();
        }

        public List<GlobalRootEntity> GetGlobalRootEntities(bool rootOnly = false)
        {
            if (rootOnly)
            {
                return GetGlobalRootEntities<GlobalRootEntity>().Where(entity => entity.ParentId == null).ToList();
            }
            return GetGlobalRootEntities<GlobalRootEntity>();
        }

        public List<T> GetGlobalRootEntities<T>() where T : GlobalRootEntity
        {
            lock (globalRootEntitiesLock)
            {
                return new(globalRootEntitiesById.Values.OfType<T>());
            }
        }

        public List<WorldEntity> GetEntities(NitroxInt3 batchId)
        {
            List<WorldEntity> result;

            lock (worldEntitiesLock)
            {
                if (!phasingEntitiesByBatchId.TryGetValue(batchId, out result))
                {
                    result = phasingEntitiesByBatchId[batchId] = new List<WorldEntity>();
                }
            }

            return result;
        }

        public List<WorldEntity> GetEntities(AbsoluteEntityCell cellId)
        {
            List<WorldEntity> result;

            lock (worldEntitiesLock)
            {
                if (!phasingEntitiesByCellId.TryGetValue(cellId, out result))
                {
                    result = phasingEntitiesByCellId[cellId] = new List<WorldEntity>();
                }
            }

            return result;
        }

        public Optional<AbsoluteEntityCell> UpdateEntityPosition(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Optional<WorldEntity> opEntity = entityRegistry.GetEntityById<WorldEntity>(id);

            if (!opEntity.HasValue)
            {
                Log.Debug("Could not update entity position because it was not found (maybe it was recently picked up)");
                return Optional.Empty;
            }

            WorldEntity entity = opEntity.Value;
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

        public void AddGlobalRootEntity(GlobalRootEntity entity, bool addToRegistry = true)
        {
            lock (globalRootEntitiesLock)
            {
                if (!globalRootEntitiesById.ContainsKey(entity.Id))
                {
                    if (addToRegistry)
                    {
                        entityRegistry.AddEntity(entity);
                        entityRegistry.AddEntitiesIgnoringDuplicate(entity.ChildEntities);
                    }
                    globalRootEntitiesById.Add(entity.Id, entity);
                }
                else
                {
                    Log.Info($"Entity Already Exists for Id: {entity.Id} Item: {entity.TechType}");
                }
            }
        }

        public Optional<Entity> RemoveGlobalRootEntity(NitroxId entityId, bool removeFromRegistry = true)
        {
            Optional<Entity> removedEntity = Optional.Empty;
            lock (globalRootEntitiesLock)
            {
                if (removeFromRegistry)
                {
                    // In case there were player entities under the removed entity, we need to reparent them to the GlobalRoot
                    // to make sure that they won't be removed
                    if (entityRegistry.TryGetEntityById(entityId, out GlobalRootEntity globalRootEntity))
                    {
                        List<PlayerWorldEntity> playerEntities = new();
                        FindPlayerEntitiesInChildrenRecursively(globalRootEntity, playerEntities);
                        foreach (PlayerWorldEntity childPlayerEntity in playerEntities)
                        {
                            // Reparent the entity on top of GlobalRoot
                            globalRootEntity.ChildEntities.Remove(childPlayerEntity);
                            childPlayerEntity.ParentId = null;

                            // Make sure the PlayerEntity is correctly registered
                            globalRootEntitiesById[childPlayerEntity.Id] = childPlayerEntity;
                            entityRegistry.AddOrUpdate(childPlayerEntity);
                        }
                    }
                    removedEntity = entityRegistry.RemoveEntity(entityId);
                }
                globalRootEntitiesById.Remove(entityId);
            }
            return removedEntity;
        }

        public void TrackEntityInTheWorld(WorldEntity entity)
        {
            if (entity is GlobalRootEntity globalRootEntity)
            {
                AddGlobalRootEntity(globalRootEntity, false);
            }
            else
            {
                lock (worldEntitiesLock)
                {
                    if (!phasingEntitiesByBatchId.TryGetValue(entity.AbsoluteEntityCell.BatchId, out List<WorldEntity> phasingEntitiesInBatch))
                    {
                        phasingEntitiesInBatch = phasingEntitiesByBatchId[entity.AbsoluteEntityCell.BatchId] = new List<WorldEntity>();
                    }

                    phasingEntitiesInBatch.Add(entity);
                }

                lock (worldEntitiesLock)
                {
                    if (!phasingEntitiesByCellId.TryGetValue(entity.AbsoluteEntityCell, out List<WorldEntity> phasingEntitiesInCell))
                    {
                        phasingEntitiesInCell = phasingEntitiesByCellId[entity.AbsoluteEntityCell] = new List<WorldEntity>();
                    }

                    phasingEntitiesInCell.Add(entity);
                }
            }
        }

        public void LoadAllUnspawnedEntities(System.Threading.CancellationToken token)
        {            
            IMap map = NitroxServiceLocator.LocateService<IMap>();

            int totalEntites = 0;

            for (int x = 0; x < map.DimensionsInBatches.X; x++)
            {
                token.ThrowIfCancellationRequested();
                for (int y = 0; y < map.DimensionsInBatches.Y; y++)
                {
                    for (int z = 0; z < map.DimensionsInBatches.Z; z++)
                    {
                        int spawned = LoadUnspawnedEntities(new(x, y, z), true);

                        Log.Debug($"Loaded {spawned} entities from batch ({x}, {y}, {z})");

                        totalEntites += spawned;
                    }
                }

                if (totalEntites > 0)
                {
                    Log.Info($"Loading: {(int)((totalEntites/ 504732.0) * 100)}%");
                }
            }
        }

        public bool IsBatchSpawned(NitroxInt3 batchId)
        {
            return batchEntitySpawner.IsBatchSpawned(batchId);
        }

        public int LoadUnspawnedEntities(NitroxInt3 batchId, bool suppressLogs)
        {
            List<Entity> spawnedEntities = batchEntitySpawner.LoadUnspawnedEntities(batchId, suppressLogs);

            List<WorldEntity> nonCellRootEntities = spawnedEntities.Where(entity => typeof(WorldEntity).IsAssignableFrom(entity.GetType()) &&
                                                                                    entity.GetType() != typeof(CellRootEntity))
                                                                   .Cast<WorldEntity>()
                                                                   .ToList();

            // UWE stores entities serialized with a handful of parent cell roots.  These only represent a small fraction of all possible cell
            // roots that could exist.  There is no reason for the server to know about these and much easier to consider top-level world entities
            // as positioned globally and not locally.  Thus, we promote cell root children to top level and throw the cell roots away. 
            foreach (CellRootEntity cellRoot in spawnedEntities.OfType<CellRootEntity>())
            {
                foreach (WorldEntity worldEntity in cellRoot.ChildEntities.Cast<WorldEntity>())
                {
                    worldEntity.ParentId = null;
                    worldEntity.Transform.SetParent(null, true);
                    nonCellRootEntities.Add(worldEntity);
                }

                cellRoot.ChildEntities = new List<Entity>();
            }

            entityRegistry.AddEntitiesIgnoringDuplicate(nonCellRootEntities.OfType<Entity>().ToList());

            foreach (WorldEntity entity in nonCellRootEntities)
            {
                List<WorldEntity> entitiesInBatch = GetEntities(entity.AbsoluteEntityCell.BatchId);
                entitiesInBatch.Add(entity);

                List<WorldEntity> entitiesInCell = GetEntities(entity.AbsoluteEntityCell);
                entitiesInCell.Add(entity);
            }

            return nonCellRootEntities.Count;
        }

        private void EntitySwitchedCells(WorldEntity entity, AbsoluteEntityCell oldCell, AbsoluteEntityCell newCell)
        {
            if (entity is GlobalRootEntity)
            {
                return; // We don't care what cell a global root entity resides in.  Only phasing entities.
            }

            if (oldCell.BatchId != newCell.BatchId)
            {
                lock (worldEntitiesLock)
                {
                    List<WorldEntity> oldList = GetEntities(oldCell.BatchId);
                    oldList.Remove(entity);

                    List<WorldEntity> newList = GetEntities(newCell.BatchId);
                    newList.Add(entity);
                }
            }

            lock (worldEntitiesLock)
            {
                List<WorldEntity> oldList = GetEntities(oldCell);
                oldList.Remove(entity);

                List<WorldEntity> newList = GetEntities(newCell);
                newList.Add(entity);
            }
        }

        public void StopTrackingEntity(WorldEntity entity)
        {
            if (entity is GlobalRootEntity)
            {
                RemoveGlobalRootEntity(entity.Id, false);
            }
            else
            {
                lock (worldEntitiesLock)
                {
                    if (phasingEntitiesByBatchId.TryGetValue(entity.AbsoluteEntityCell.BatchId, out List<WorldEntity> batchEntities))
                    {
                        batchEntities.Remove(entity);
                    }
                }

                lock (worldEntitiesLock)
                {
                    if (phasingEntitiesByCellId.TryGetValue(entity.AbsoluteEntityCell, out List<WorldEntity> cellEntities))
                    {
                        cellEntities.Remove(entity);
                    }
                }
            }
        }

        public bool TryDestroyEntity(NitroxId entityId, out Optional<Entity> entity)
        {
            entity = entityRegistry.RemoveEntity(entityId);

            if (!entity.HasValue)
            {
                return false;
            }

            if (entity.Value is WorldEntity worldEntity)
            {
                StopTrackingEntity(worldEntity);
            }

            return true;
        }

        /// <summary>
        /// To avoid risking not having the same entity in <see cref="globalRootEntitiesById"/> and in EntityRegistry, we update both at the same time.
        /// </summary>
        public void UpdateGlobalRootEntity(GlobalRootEntity entity)
        {
            lock (globalRootEntitiesLock)
            {
                entityRegistry.AddOrUpdate(entity);
                globalRootEntitiesById[entity.Id] = entity;
            }
        }

        private void FindPlayerEntitiesInChildrenRecursively(Entity parentEntity, List<PlayerWorldEntity> playerEntities)
        {
            foreach (Entity childEntity in parentEntity.ChildEntities)
            {
                if (childEntity is PlayerWorldEntity playerWorldEntity)
                {
                    playerEntities.Add(playerWorldEntity);
                    continue;
                }
                FindPlayerEntitiesInChildrenRecursively(childEntity, playerEntities);
            }
        }
    }
}
