using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;

        private readonly Dictionary<NitroxId, Type> spawnedAsType = new();
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = new Dictionary<Type, IEntitySpawner>();

        public Entities(IPacketSender packetSender, ThrottledPacketSender throttledPacketSender, PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
        {
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;

            entitySpawnersByType[typeof(PrefabChildEntity)] = new PrefabChildEntitySpawner();
            entitySpawnersByType[typeof(PathBasedChildEntity)] = new PathBasedChildEntitySpawner();
            entitySpawnersByType[typeof(InstalledModuleEntity)] = new InstalledModuleEntitySpawner();
            entitySpawnersByType[typeof(InstalledBatteryEntity)] = new InstalledBatteryEntitySpawner();
            entitySpawnersByType[typeof(InventoryEntity)] = new InventoryEntitySpawner();
            entitySpawnersByType[typeof(InventoryItemEntity)] = new InventoryItemEntitySpawner();
            entitySpawnersByType[typeof(WorldEntity)] = new WorldEntitySpawner(playerManager, localPlayer, this);
            entitySpawnersByType[typeof(PlaceholderGroupWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(EscapePodWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(PlayerWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(VehicleWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(GlobalRootEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(BuildEntity)] = new BuildEntitySpawner(this);
            entitySpawnersByType[typeof(ModuleEntity)] = new ModuleEntitySpawner(this);
            entitySpawnersByType[typeof(GhostEntity)] = new GhostEntitySpawner();
            entitySpawnersByType[typeof(InteriorPieceEntity)] = new InteriorPieceEntitySpawner(this);
        }

        public void EntityMetadataChanged(object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdate(id, metadata.Value);
            }
        }

        public void EntityMetadataChangedThrottled(object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdateThrottled(id, metadata.Value);
            }
        }

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            packetSender.Send(new EntityMetadataUpdate(id, metadata));
        }

        public void BroadcastMetadataUpdateThrottled(NitroxId id, EntityMetadata metadata)
        {
            throttledPacketSender.SendThrottled(new EntityMetadataUpdate(id, metadata), (packet) => ((EntityMetadataUpdate)packet).Id);
        }

        public void BroadcastEntitySpawnedByClient(WorldEntity entity)
        {
            packetSender.Send(new EntitySpawnedByClient(entity));
        }

        public IEnumerator SpawnAsync(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (WasAlreadySpawned(entity))
                {
                    if (entity is WorldEntity worldEntity)
                    {
                        UpdatePosition(worldEntity);
                    }
                }
                else if (entity.ParentId != null && !IsParentReady(entity.ParentId))
                {
                    AddPendingParentEntity(entity);
                }
                else
                {
                    yield return SpawnAsync(entity).OnYieldError(Log.Error);
                }
            }
        }

        public IEnumerator SpawnAsync(Entity entity)
        {
            MarkAsSpawned(entity);

            IEntitySpawner entitySpawner = entitySpawnersByType[entity.GetType()];

            TaskResult<Optional<GameObject>> gameObjectTaskResult = new TaskResult<Optional<GameObject>>();
            yield return entitySpawner.SpawnAsync(entity, gameObjectTaskResult);
            Optional<GameObject> gameObject = gameObjectTaskResult.Get();

            if (gameObject.HasValue)
            {
                if (!entitySpawner.SpawnsOwnChildren(entity))
                {
                    yield return SpawnChildren(entity);
                }

                // Apply entity metadata after children have been spawned.  This will allow metadata processors to
                // interact with children if necessary (for example, PlayerMetadata which equips inventory items).
                EntityMetadataProcessor.ApplyMetadata(gameObject.Value, entity.Metadata);
            }
        }

        /// <remarks>
        /// Yield returning takes too much time and it quickly gets out of hand with long function call hierarchies so
        /// we want to reduce the amount of yield operations and only skip to the next frame when required (to maintain the FPS)
        /// </remarks>
        /// <param name="forceRespawn">Should children be spawned even if already marked as spawned</param>
        public IEnumerator SpawnBatchAsync<T>(IEnumerable<T> batch, bool forceRespawn = false) where T : Entity
        {
            int timeSkips = 0;
            
            float allottedTimePerFrame = 1f / Application.targetFrameRate;
            float timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            TaskResult<Optional<GameObject>> entityResult = new();
            TaskResult<Exception> exception = new();
            foreach (Entity entity in batch)
            {
                entityResult.Set(Optional.Empty);
                exception.Set(null);

                IEntitySpawner entitySpawner = entitySpawnersByType[entity.GetType()];

                if (!entitySpawner.SpawnSyncSafe(entity, entityResult, exception) && exception.Get() == null)
                {
                    IEnumerator coroutine = entitySpawner.SpawnAsync(entity, entityResult);
                    if (coroutine != null)
                    {
                        yield return CoroutineUtils.YieldSafe(coroutine, exception);
                        timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                    }
                    else
                    {
                        Log.Error($"Spawn coroutine for {entity.Id} is null");
                        continue;
                    }
                }

                if (exception.Get() != null)
                {
                    Log.Error(exception.Get());
                    continue;
                }
                else if (!entityResult.Get().Value)
                {
                    continue;
                }

                MarkAsSpawned(entity);

                List<Entity> childrenToSpawn = GetChildrenRecursively(entity, forceRespawn);
                List<NitroxId> childrenIds = childrenToSpawn.Select(entity => entity.Id).ToList();
                if (!entitySpawner.SpawnsOwnChildren(entity) &&
                    pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
                {
                    childrenToSpawn.AddRange(pendingEntities.Where(pendingEntity => !childrenIds.Contains(pendingEntity.Id)));
                    pendingParentEntitiesByParentId.Remove(entity.Id);
                }

                foreach (Entity childEntity in childrenToSpawn)
                {
                    exception.Set(null);
                    TaskResult<Optional<GameObject>> childResult = new();
                    IEntitySpawner childSpawner = entitySpawnersByType[childEntity.GetType()];
                    if (!childSpawner.SpawnSyncSafe(childEntity, childResult, exception) && exception.Get() == null)
                    {
                        IEnumerator coroutine = childSpawner.SpawnAsync(childEntity, childResult);
                        if (coroutine != null)
                        {
                            yield return CoroutineUtils.YieldSafe(coroutine, exception);
                            timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                        }
                        else
                        {
                            Log.Error($"Spawn coroutine for {entity.Id} is null");
                            continue;
                        }
                    }

                    if (exception.Get() != null)
                    {
                        Log.Error(exception.Get());
                        continue;
                    }
                    else if (!childResult.Get().Value)
                    {
                        continue;
                    }

                    MarkAsSpawned(childEntity);
                    Optional<GameObject> childGameObject = childResult.Get();

                    if (childGameObject.HasValue)
                    {
                        // Apply entity metadata after children have been spawned.  This will allow metadata processors to
                        // interact with children if necessary (for example, PlayerMetadata which equips inventory items).
                        EntityMetadataProcessor.ApplyMetadata(childGameObject.Value, childEntity.Metadata);
                    }

                    if (Time.realtimeSinceStartup > timeLimit)
                    {
                        yield return null;
                        timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                        timeSkips++;
                    }
                }

                // Apply entity metadata after children have been spawned.  This will allow metadata processors to
                // interact with children if necessary (for example, PlayerMetadata which equips inventory items).
                EntityMetadataProcessor.ApplyMetadata(entityResult.Get().Value, entity.Metadata);

                if (Time.realtimeSinceStartup > timeLimit)
                {
                    yield return null;
                    timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                    timeSkips++;
                }
            }
#if DEBUG
            Log.Verbose($"Optimized spawning took {stopwatch.ElapsedMilliseconds}ms with {timeSkips} time skips");
#endif
        }

        public List<Entity> GetChildrenRecursively(Entity entity, bool forceRespawn = false)
        {
            if (entitySpawnersByType[entity.GetType()].SpawnsOwnChildren(entity))
            {
                return new();
            }
            List<Entity> children = new(entity.ChildEntities);
            foreach (Entity child in entity.ChildEntities)
            {
                if (forceRespawn)
                {
                    children.AddRange(GetChildrenRecursively(child, forceRespawn));
                }
                else
                {
                    children.AddRange(GetChildrenRecursively(child, forceRespawn)
                            .Where(child => !WasAlreadySpawned(child)));
                }
            }
            return children;
        }

        private IEnumerator SpawnChildren(Entity entity)
        {
            foreach (Entity childEntity in entity.ChildEntities)
            {
                if (!WasAlreadySpawned(childEntity))
                {
                    yield return SpawnAsync(childEntity);
                }
            }

            if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
            {
                foreach (WorldEntity child in pendingEntities)
                {
                    if (!WasAlreadySpawned(child))
                    {
                        yield return SpawnAsync(child);
                    }
                }

                pendingParentEntitiesByParentId.Remove(entity.Id);
            }
        }

        private void UpdatePosition(WorldEntity entity)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(entity.Id);

            if (!opGameObject.HasValue)
            {
#if DEBUG && ENTITY_LOG
                Log.Error($"Entity was already spawned but not found(is it in another chunk?) NitroxId: {entity.Id} TechType: {entity.TechType} ClassId: {entity.ClassId} Transform: {entity.Transform}");
#endif
                return;
            }

            opGameObject.Value.transform.position = entity.Transform.Position.ToUnity();
            opGameObject.Value.transform.rotation = entity.Transform.Rotation.ToUnity();
            opGameObject.Value.transform.localScale = entity.Transform.LocalScale.ToUnity();
        }

        private void AddPendingParentEntity(Entity entity)
        {
            if (!pendingParentEntitiesByParentId.TryGetValue(entity.ParentId, out List<Entity> pendingEntities))
            {
                pendingEntities = new List<Entity>();
                pendingParentEntitiesByParentId[entity.ParentId] = pendingEntities;
            }

            pendingEntities.Add(entity);
        }

        // Entites can sometimes be spawned as one thing but need to be respawned later as another.  For example, a flare
        // spawned inside an Inventory as an InventoryItemEntity can later be dropped in the world as a WorldEntity. Another
        // example would be a base ghost that needs to be respawned a completed piece. 
        public bool WasAlreadySpawned(Entity entity)
        {
            if (spawnedAsType.TryGetValue(entity.Id, out Type type))
            {
                return (type == entity.GetType());
            }

            return false;
        }

        public bool IsKnownEntity(NitroxId id)
        {
            return spawnedAsType.ContainsKey(id);
        }

        public Type RequireEntityType(NitroxId id)
        {
            if (spawnedAsType.TryGetValue(id, out Type type))
            {
                return type;
            }

            throw new InvalidOperationException($"Did not have a type for {id}");
        }

        public bool IsParentReady(NitroxId id)
        {
            return WasParentSpawned(id) || (NitroxEntity.TryGetObjectFrom(id, out GameObject o) && o);
        }

        public bool WasParentSpawned(NitroxId id)
        {
            return spawnedAsType.ContainsKey(id);
        }

        public void MarkAsSpawned(Entity entity)
        {
            spawnedAsType[entity.Id] = entity.GetType();
        }

        public bool RemoveEntity(NitroxId id) => spawnedAsType.Remove(id);

        /// <summary>
        /// Allows the ability to respawn an entity and its entire hierarchy. Callers are responsible for ensuring the
        /// entity is no longer in the world.
        /// </summary>
        public void RemoveEntityHierarchy(Entity entity)
        {
            RemoveEntity(entity.Id);

            foreach (Entity child in entity.ChildEntities)
            {
                RemoveEntityHierarchy(child);
            }
        }
    }
}
