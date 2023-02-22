using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.GameLogic.Entities
{
    public class WorldEntityManager
    {
        private readonly EntityRegistry entityRegistry;
        private readonly EntitySimulation entitySimulation;

        /// <summary>
        ///     Phasing entities can disappear if you go out of range.
        /// </summary>
        private readonly Dictionary<AbsoluteEntityCell, List<WorldEntity>> phasingEntitiesByAbsoluteCell;

        /// <summary>
        ///     Global root entities that are always visible.
        /// </summary>
        private readonly Dictionary<NitroxId, WorldEntity> globalRootEntitiesById;

        private readonly BatchEntitySpawner batchEntitySpawner;

        public WorldEntityManager(EntityRegistry entityRegistry, BatchEntitySpawner batchEntitySpawner)
        {
            List<WorldEntity> worldEntities = entityRegistry.GetEntities<WorldEntity>();

            globalRootEntitiesById = worldEntities.Where(entity => entity.ExistsInGlobalRoot)
                                                  .ToDictionary(entity => entity.Id);

            phasingEntitiesByAbsoluteCell = worldEntities.Where(entity => !entity.ExistsInGlobalRoot)
                                                         .GroupBy(entity => entity.AbsoluteEntityCell)
                                                         .ToDictionary(group => group.Key, group => group.ToList());
            this.entityRegistry = entityRegistry;
            this.batchEntitySpawner = batchEntitySpawner;
        }

        public List<WorldEntity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            LoadUnspawnedEntities(cells);

            List<WorldEntity> entities = new List<WorldEntity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<WorldEntity> cellEntities = GetEntities(cell);
                entities.AddRange(cellEntities.Where(entity => cell.Level <= entity.Level));
            }

            return entities;
        }

        public List<WorldEntity> GetGlobalRootEntities()
        {
            lock (globalRootEntitiesById)
            {
                return new List<WorldEntity>(globalRootEntitiesById.Values);
            }
        }

        public List<WorldEntity> GetEntities(AbsoluteEntityCell absoluteEntityCell)
        {
            List<WorldEntity> result;

            lock (phasingEntitiesByAbsoluteCell)
            {
                if (!phasingEntitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = phasingEntitiesByAbsoluteCell[absoluteEntityCell] = new List<WorldEntity>();
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

        public void TrackEntityInTheWorld(WorldEntity entity)
        {
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
                        Log.Info($"Entity Already Exists for Id: {entity.Id} Item: {entity.TechType}");
                    }
                }
            }
            else
            {
                lock (phasingEntitiesByAbsoluteCell)
                {
                    if (!phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out List<WorldEntity> phasingEntitiesInCell))
                    {
                        phasingEntitiesInCell = phasingEntitiesByAbsoluteCell[entity.AbsoluteEntityCell] = new List<WorldEntity>();
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
                        int spawned = LoadFromBatch(new(x, y, z), true);

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

        private void LoadUnspawnedEntities(AbsoluteEntityCell[] cells)
        {
            IEnumerable<NitroxInt3> distinctBatchIds = cells.Select(cell => cell.BatchId).Distinct();

            foreach (NitroxInt3 batchId in distinctBatchIds)
            {
                LoadFromBatch(batchId, false);
            }
        }

        private int LoadFromBatch(NitroxInt3 batchId, bool suppressLogs)
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

            entityRegistry.AddEntitiesIgnoringDuplicate(nonCellRootEntities.Cast<Entity>().ToList());

            foreach (WorldEntity entity in nonCellRootEntities)
            {
                List<WorldEntity> entitiesInCell = GetEntities(entity.AbsoluteEntityCell);
                entitiesInCell.Add(entity);
            }

            return nonCellRootEntities.Count;
        }

        private void EntitySwitchedCells(WorldEntity entity, AbsoluteEntityCell oldCell, AbsoluteEntityCell newCell)
        {
            if (entity.ExistsInGlobalRoot)
            {
                return; // We don't care what cell a global root entity resides in.  Only phasing entities.
            }

            lock (phasingEntitiesByAbsoluteCell)
            {
                List<WorldEntity> oldList = GetEntities(oldCell);
                oldList.Remove(entity);

                List<WorldEntity> newList = GetEntities(newCell);
                newList.Add(entity);
            }
        }

        public void StopTrackingEntity(WorldEntity entity)
        {
            if (entity.ExistsInGlobalRoot)
            {
                lock (globalRootEntitiesById)
                {
                    globalRootEntitiesById.Remove(entity.Id);
                }
            }
            else
            {
                lock (phasingEntitiesByAbsoluteCell)
                {
                    if (phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out List<WorldEntity> entities))
                    {
                        entities.Remove(entity);
                    }
                }
            }
        }

        public bool TryDestroyEntity(NitroxId entityId, out Optional<Entity> entity)
        {
            entitySimulation.EntityDestroyed(entityId);

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
    }
}
