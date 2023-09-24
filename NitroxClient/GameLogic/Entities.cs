using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Abstract;
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

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;

        private readonly Dictionary<NitroxId, Type> spawnedAsType = new();
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = new Dictionary<Type, IEntitySpawner>();

#if DEBUG
        public int SpawningEntitiesCount;
#endif

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

        /// <remarks>
        /// Yield returning takes too much time (at least once per IEnumerator branch) and it quickly gets out of hand with long function call hierarchies so
        /// we want to reduce the amount of yield operations and only skip to the next frame when required (to maintain the FPS).
        /// Also saves resources by using the IOut instances
        /// </remarks>
        /// <param name="forceRespawn">Should children be spawned even if already marked as spawned</param>
        public IEnumerator SpawnBatchAsync(IEnumerable<Entity> batch, bool forceRespawn = false, bool skipFrames = true)
        {
            Dictionary<NitroxTechType, List<float>> distances = new(batch.Count());

            int frameSkips = 0;
            int syncExecutions = 0;
            int asyncExecutions = 0;
            
            // we divide the FPS by 2.5 because we consider (time for 1 frame + spawning time without a frame + extra computing time)
            float allottedTimePerFrame = 0.4f / Application.targetFrameRate;
            float timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
            Log.Info($"Current time: {Time.realtimeSinceStartup}, allottedTimePerFrame: {allottedTimePerFrame}, timeLimit: {timeLimit}");

#if DEBUG
            SpawningEntitiesCount += batch.Count();
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            TaskResult<Optional<GameObject>> entityResult = new();
            TaskResult<Exception> exception = new();
            Stack<Entity> stack = new(batch);
            while (stack.Count > 0)
            {
                Entity entity = stack.Pop();
#if DEBUG
                SpawningEntitiesCount--;
#endif
                entityResult.Set(Optional.Empty);
                exception.Set(null);

                // Preconditions which may get the spawn process cancelled or postponed
                if (WasAlreadySpawned(entity))
                {
                    if (entity is WorldEntity worldEntity)
                    {
                        UpdatePosition(worldEntity);
                    }
                    continue;
                }
                else if (entity.ParentId != null && !IsParentReady(entity.ParentId))
                {
                    AddPendingParentEntity(entity);
                    continue;
                }

                // Executing the spawn instructions whether they're sync or async
                IEntitySpawner entitySpawner = entitySpawnersByType[entity.GetType()];
                if (entitySpawner is not ISyncEntitySpawner syncEntitySpawner ||
                    (!syncEntitySpawner.SpawnSyncSafe(entity, entityResult, exception) && exception.Get() == null))
                {
                    IEnumerator coroutine = entitySpawner.SpawnAsync(entity, entityResult);
                    if (coroutine != null)
                    {
                        yield return coroutine.OnYieldError(Log.Error);
                        //timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                        asyncExecutions++;
                    }
                    else
                    {
                        syncExecutions++;
                    }
                }

                // Any error in there would make spawning children useless
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

                EntityMetadataProcessor.ApplyMetadata(entityResult.Get().Value, entity.Metadata);

                // Finding out about all children (can be hidden in the object's hierarchy or in a pending list)
                
                if (!entitySpawner.SpawnsOwnChildren(entity))
                {
                    SpawningEntitiesCount += entity.ChildEntities.Count;
                    foreach (Entity childEntity in entity.ChildEntities)
                    {
                        stack.Push(childEntity);
                    }
                    List<NitroxId> childrenIds = entity.ChildEntities.Select(entity => entity.Id).ToList();
                    if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
                    {
                        foreach (Entity pendingEntity in pendingEntities.Where(e => !childrenIds.Contains(e.Id)))
                        {
                            SpawningEntitiesCount++;
                            stack.Push(pendingEntity);
                        }
                        pendingParentEntitiesByParentId.Remove(entity.Id);
                    }
                }
                
                // Skip a frame to maintain FPS
                if (Time.realtimeSinceStartup > timeLimit && skipFrames)
                {
                    yield return new WaitForEndOfFrame();
                    timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                    frameSkips++;
                }
            }
#if DEBUG
            stopwatch.Stop();
            Log.Verbose($"Optimized spawning took {stopwatch.ElapsedMilliseconds}ms with {frameSkips} frame skips, {asyncExecutions}/{syncExecutions} async/sync executions.");
#endif
        }

        public IEnumerator SpawnEntityAsync(Entity entity, bool forceRespawn = false)
        {
            return SpawnBatchAsync(Enumerable.Repeat(entity, 1), forceRespawn, skipFrames: false);
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
