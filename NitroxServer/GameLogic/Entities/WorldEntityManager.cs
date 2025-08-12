using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.GameLogic.Entities;

/// <remarks>
/// Regular <see cref="WorldEntity"/> are held in cells and should be registered in <see cref="worldEntitiesByBatchId"/> and <see cref="worldEntitiesByCell"/>.
/// But <see cref="GlobalRootEntity"/> are held in their own root object (GlobalRoot) so they should never be registered in cells (they're seeable at all times).
/// </remarks>
public class WorldEntityManager
{
    private readonly EntityRegistry entityRegistry;

    /// <summary>
    ///     World entities can disappear if you go out of range.
    /// </summary>
    private readonly Dictionary<AbsoluteEntityCell, Dictionary<NitroxId, WorldEntity>> worldEntitiesByCell;

    /// <summary>
    ///     Global root entities that are always visible.
    /// </summary>
    private readonly Dictionary<NitroxId, GlobalRootEntity> globalRootEntitiesById;

    private readonly BatchEntitySpawner batchEntitySpawner;
    private readonly PlayerManager playerManager;

    private readonly object worldEntitiesLock;
    private readonly object globalRootEntitiesLock;

    public WorldEntityManager(EntityRegistry entityRegistry, BatchEntitySpawner batchEntitySpawner, PlayerManager playerManager)
    {
        List<WorldEntity> worldEntities = entityRegistry.GetEntities<WorldEntity>();

        globalRootEntitiesById = entityRegistry.GetEntities<GlobalRootEntity>().ToDictionary(entity => entity.Id);

        worldEntitiesByCell = worldEntities.Where(entity => entity is not GlobalRootEntity)
                                               .GroupBy(entity => entity.AbsoluteEntityCell)
                                               .ToDictionary(group => group.Key, group => group.ToDictionary(entity => entity.Id, entity => entity));
        this.entityRegistry = entityRegistry;
        this.batchEntitySpawner = batchEntitySpawner;
        this.playerManager = playerManager;

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

    public List<GlobalRootEntity> GetPersistentGlobalRootEntities()
    {
        // TODO: refactor if there are more entities that should not be persisted
        return GetGlobalRootEntities(true).Where(entity =>
        {
            if (entity.Metadata is CyclopsMetadata cyclopsMetadata)
            {
                // Do not save cyclops wrecks
                return !cyclopsMetadata.IsDestroyed;
            }

            return true;
        }).ToList();
    }

    public List<WorldEntity> GetEntities(AbsoluteEntityCell cell)
    {
        lock (worldEntitiesLock)
        {
            if (worldEntitiesByCell.TryGetValue(cell, out Dictionary<NitroxId, WorldEntity> batchEntities))
            {
                return batchEntities.Values.ToList();
            }
        }

        return [];
    }

    public bool TryUpdateEntityPosition(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, out AbsoluteEntityCell newCell, out WorldEntity worldEntity)
    {
        lock (worldEntitiesLock)
        {
            if (!entityRegistry.TryGetEntityById(id, out worldEntity))
            {
                Log.WarnOnce($"[{nameof(WorldEntityManager)}] Can't update entity position of {id} because it isn't registered");
                newCell = null;
                return false;
            }

            AbsoluteEntityCell oldCell = worldEntity.AbsoluteEntityCell;

            worldEntity.Transform.Position = position;
            worldEntity.Transform.Rotation = rotation;

            newCell = worldEntity.AbsoluteEntityCell;
            
            if (oldCell != newCell)
            {
                EntitySwitchedCells(worldEntity, oldCell, newCell);
            }

            return true;
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
                    MovePlayerChildrenToRoot(globalRootEntity);
                }
                removedEntity = entityRegistry.RemoveEntity(entityId);
            }
            globalRootEntitiesById.Remove(entityId);
        }
        return removedEntity;
    }

    public void MovePlayerChildrenToRoot(GlobalRootEntity globalRootEntity)
    {
        List<PlayerWorldEntity> playerEntities = FindPlayerEntitiesInChildren(globalRootEntity);
        foreach (PlayerWorldEntity childPlayerEntity in playerEntities)
        {
            // Reparent the entity on top of GlobalRoot
            globalRootEntity.ChildEntities.Remove(childPlayerEntity);
            childPlayerEntity.ParentId = null;

            // Make sure the PlayerEntity is correctly registered
            AddOrUpdateGlobalRootEntity(childPlayerEntity);
        }
    }

    public void TrackEntityInTheWorld(WorldEntity entity)
    {
        if (entity is GlobalRootEntity globalRootEntity)
        {
            AddOrUpdateGlobalRootEntity(globalRootEntity, false);
            return;
        }

        RegisterWorldEntity(entity);
    }

    /// <summary>
    /// Automatically registers a WorldEntity in its AbsoluteEntityCell
    /// </summary>
    /// <remarks>
    /// The provided should NOT be a GlobalRootEntity (they don't stand in cells)
    /// </remarks>
    public void RegisterWorldEntity(WorldEntity entity)
    {
        RegisterWorldEntityInCell(entity, entity.AbsoluteEntityCell);
    }

    public void RegisterWorldEntityInCell(WorldEntity entity, AbsoluteEntityCell cell)
    {
        lock (worldEntitiesLock)
        {
            if (!worldEntitiesByCell.TryGetValue(cell, out Dictionary<NitroxId, WorldEntity> worldEntitiesInCell))
            {
                worldEntitiesInCell = worldEntitiesByCell[cell] = [];
            }
            worldEntitiesInCell[entity.Id] = entity;
        }
    }

    /// <summary>
    /// Automatically unregisters a WorldEntity in its AbsoluteEntityCell
    /// </summary>
    public void UnregisterWorldEntity(WorldEntity entity)
    {
        UnregisterWorldEntityFromCell(entity.Id, entity.AbsoluteEntityCell);
    }

    public void UnregisterWorldEntityFromCell(NitroxId entityId, AbsoluteEntityCell cell)
    {
        lock (worldEntitiesLock)
        {
            if (worldEntitiesByCell.TryGetValue(cell, out Dictionary<NitroxId, WorldEntity> worldEntitiesInCell))
            {
                worldEntitiesInCell.Remove(entityId);
            }
        }
    }

    public void LoadAllUnspawnedEntities(System.Threading.CancellationToken token)
    {            
        IMap map = NitroxServiceLocator.LocateService<IMap>();

        int totalBatches = map.DimensionsInBatches.X * map.DimensionsInBatches.Y * map.DimensionsInBatches.Z;
        int batchesLoaded = 0;

        for (int x = 0; x < map.DimensionsInBatches.X; x++)
        {
            token.ThrowIfCancellationRequested();
            for (int y = 0; y < map.DimensionsInBatches.Y; y++)
            {
                for (int z = 0; z < map.DimensionsInBatches.Z; z++)
                {
                    int spawned = LoadUnspawnedEntities(new(x, y, z), true);

                    Log.Debug($"Loaded {spawned} entities from batch ({x}, {y}, {z})");

                    batchesLoaded++;
                }
            }

            if (batchesLoaded > 0)
            {
                Log.Info($"Loading : {(int)(100f * batchesLoaded / totalBatches)}%");
            }
        }
    }

    public int LoadUnspawnedEntities(NitroxInt3 batchId, bool suppressLogs)
    {
        List<Entity> spawnedEntities = batchEntitySpawner.LoadUnspawnedEntities(batchId, suppressLogs);

        List<WorldEntity> entitiesInCells = spawnedEntities.Where(entity => typeof(WorldEntity).IsAssignableFrom(entity.GetType()) &&
                                                                                entity.GetType() != typeof(CellRootEntity) &&
                                                                                entity.GetType() != typeof(GlobalRootEntity))
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
                entitiesInCells.Add(worldEntity);
            }

            cellRoot.ChildEntities = new List<Entity>();
        }
        // Specific type of entities which is not parented to a CellRootEntity
        entitiesInCells.AddRange(spawnedEntities.OfType<SerializedWorldEntity>());

        entityRegistry.AddEntitiesIgnoringDuplicate(entitiesInCells.OfType<Entity>().ToList());

        foreach (WorldEntity entity in entitiesInCells)
        {
            RegisterWorldEntity(entity);
        }

        return entitiesInCells.Count;
    }

    private void EntitySwitchedCells(WorldEntity entity, AbsoluteEntityCell oldCell, AbsoluteEntityCell newCell)
    {
        if (entity is GlobalRootEntity)
        {
            return; // We don't care what cell a global root entity resides in.  Only phasing entities.
        }

        if (oldCell != newCell)
        {
            lock (worldEntitiesLock)
            {
                // Specifically remove entity from oldCell
                UnregisterWorldEntityFromCell(entity.Id, oldCell);

                // Automatically add entity to its new cell
                RegisterWorldEntityInCell(entity, newCell);
                
                // It can happen for some players that the entity moves to a loaded cell of theirs, but that they hadn't spawned it in the first place
                foreach (Player player in playerManager.ConnectedPlayers())
                {
                    if (player.HasCellLoaded(newCell) && !player.HasCellLoaded(oldCell))
                    {
                        player.SendPacket(new SpawnEntities(entity));
                    }
                }
            }
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
            UnregisterWorldEntity(entity);
        }
    }

    public bool TryDestroyEntity(NitroxId entityId, out Entity entity)
    {
        Optional<Entity> optEntity = entityRegistry.RemoveEntity(entityId);

        if (!optEntity.HasValue)
        {
            entity = null;
            return false;
        }
        entity = optEntity.Value;

        if (entity is WorldEntity worldEntity)
        {
            StopTrackingEntity(worldEntity);
        }

        return true;
    }

    /// <summary>
    /// To avoid risking not having the same entity in <see cref="globalRootEntitiesById"/> and in EntityRegistry, we update both at the same time.
    /// </summary>
    public void AddOrUpdateGlobalRootEntity(GlobalRootEntity entity, bool addOrUpdateRegistry = true)
    {
        lock (globalRootEntitiesLock)
        {
            if (addOrUpdateRegistry)
            {
                entityRegistry.AddOrUpdate(entity);
            }
            globalRootEntitiesById[entity.Id] = entity;
        }
    }

    /// <summary>
    /// Iterative breadth-first search which gets all children player entities in <paramref name="parentEntity"/>'s hierarchy.
    /// </summary>
    private List<PlayerWorldEntity> FindPlayerEntitiesInChildren(Entity parentEntity)
    {
        List<PlayerWorldEntity> playerWorldEntities = [];
        List<Entity> entitiesToSearch = [parentEntity];

        while (entitiesToSearch.Count > 0)
        {
            Entity currentEntity = entitiesToSearch[^1];
            entitiesToSearch.RemoveAt(entitiesToSearch.Count - 1);

            if (currentEntity is PlayerWorldEntity playerWorldEntity)
            {
                playerWorldEntities.Add(playerWorldEntity);
            }
            else
            {
                entitiesToSearch.InsertRange(0, currentEntity.ChildEntities);
            }
        }
        return playerWorldEntities;
    }
}
