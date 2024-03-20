using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;
        private readonly EntityMetadataManager entityMetadataManager;

        private readonly Dictionary<NitroxId, Type> spawnedAsType = new();
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = new Dictionary<Type, IEntitySpawner>();

        public List<Entity> EntitiesToSpawn { get; private init; }
        private bool spawningEntities;

        private readonly HashSet<NitroxId> deletedEntitiesIds = new();

        public Entities(IPacketSender packetSender, ThrottledPacketSender throttledPacketSender, EntityMetadataManager entityMetadataManager, PlayerManager playerManager, ILocalNitroxPlayer localPlayer, LiveMixinManager liveMixinManager, TimeManager timeManager, SimulationOwnership simulationOwnership)
        {
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;
            this.entityMetadataManager = entityMetadataManager;
            EntitiesToSpawn = new();

            entitySpawnersByType[typeof(PrefabChildEntity)] = new PrefabChildEntitySpawner();
            entitySpawnersByType[typeof(PathBasedChildEntity)] = new PathBasedChildEntitySpawner();
            entitySpawnersByType[typeof(InstalledModuleEntity)] = new InstalledModuleEntitySpawner();
            entitySpawnersByType[typeof(InstalledBatteryEntity)] = new InstalledBatteryEntitySpawner();
            entitySpawnersByType[typeof(InventoryEntity)] = new InventoryEntitySpawner();
            entitySpawnersByType[typeof(InventoryItemEntity)] = new InventoryItemEntitySpawner();
            entitySpawnersByType[typeof(WorldEntity)] = new WorldEntitySpawner(entityMetadataManager, playerManager, localPlayer, this, simulationOwnership);
            entitySpawnersByType[typeof(PlaceholderGroupWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(PrefabPlaceholderEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(EscapePodWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(PlayerWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(VehicleWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(SerializedWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(GlobalRootEntity)] = new GlobalRootEntitySpawner();
            entitySpawnersByType[typeof(BaseLeakEntity)] = new BaseLeakEntitySpawner(liveMixinManager);
            entitySpawnersByType[typeof(BuildEntity)] = new BuildEntitySpawner(this, (BaseLeakEntitySpawner)entitySpawnersByType[typeof(BaseLeakEntity)]);
            entitySpawnersByType[typeof(RadiationLeakEntity)] = new RadiationLeakEntitySpawner(timeManager);
            entitySpawnersByType[typeof(ModuleEntity)] = new ModuleEntitySpawner(this);
            entitySpawnersByType[typeof(GhostEntity)] = new GhostEntitySpawner();
            entitySpawnersByType[typeof(OxygenPipeEntity)] = new OxygenPipeEntitySpawner(this, (WorldEntitySpawner)entitySpawnersByType[typeof(WorldEntity)]);
            entitySpawnersByType[typeof(PlacedWorldEntity)] = new PlacedWorldEntitySpawner((WorldEntitySpawner)entitySpawnersByType[typeof(WorldEntity)]);
            entitySpawnersByType[typeof(InteriorPieceEntity)] = new InteriorPieceEntitySpawner(this, entityMetadataManager);
            entitySpawnersByType[typeof(GeyserWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(ReefbackEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(ReefbackChildEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(CreatureRespawnEntity)] = entitySpawnersByType[typeof(WorldEntity)];
        }

        public void EntityMetadataChanged(object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdate(id, metadata.Value);
            }
        }

        public void EntityMetadataChangedThrottled(object o, NitroxId id, float throttleTime = 0.2f)
        {
            // As throttled broadcasting is done after some time by a different function, this is where the packet sending should be interrupted
            if (PacketSuppressor<EntityMetadataUpdate>.IsSuppressed)
            {
                return;
            }
            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdateThrottled(id, metadata.Value, throttleTime);
            }
        }

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            packetSender.Send(new EntityMetadataUpdate(id, metadata));
        }

        public void BroadcastMetadataUpdateThrottled(NitroxId id, EntityMetadata metadata, float throttleTime = 0.2f)
        {
            throttledPacketSender.SendThrottled(new EntityMetadataUpdate(id, metadata), packet => packet.Id, throttleTime);
        }

        public void BroadcastEntitySpawnedByClient(Entity entity, bool requireRespawn = false)
        {
            packetSender.Send(new EntitySpawnedByClient(entity, requireRespawn));
        }

        private IEnumerator SpawnNewEntities()
        {
            bool restarted = false;
            yield return SpawnBatchAsync(EntitiesToSpawn).OnYieldError(exception =>
            {
                Log.Error(exception);
                if (EntitiesToSpawn.Count > 0)
                {
                    restarted = true;
                    // It's safe to run a new time because the processed entity is removed first so it won't infinitely throw errors
                    CoroutineHost.StartCoroutine(SpawnNewEntities());
                }
            });
            spawningEntities = restarted;
            if (!spawningEntities)
            {
                entityMetadataManager.ClearNewerMetadata();
                deletedEntitiesIds.Clear();
            }
        }

        public void EnqueueEntitiesToSpawn(List<Entity> entitiesToEnqueue)
        {
            EntitiesToSpawn.InsertRange(0, entitiesToEnqueue);
            if (!spawningEntities)
            {
                spawningEntities = true;
                CoroutineHost.StartCoroutine(SpawnNewEntities());
            }
        }

        /// <remarks>
        /// Yield returning takes too much time (at least once per IEnumerator branch) and it quickly gets out of hand with long function call hierarchies so
        /// we want to reduce the amount of yield operations and only skip to the next frame when required (to maintain the FPS).
        /// Also saves resources by using the IOut instances
        /// </remarks>
        /// <param name="forceRespawn">Should children be spawned even if already marked as spawned</param>
        public IEnumerator SpawnBatchAsync(List<Entity> batch, bool forceRespawn = false, bool skipFrames = true)
        {
            // we divide the FPS by 2.5 because we consider (time for 1 frame + spawning time without a frame + extra computing time)
            float allottedTimePerFrame = 0.4f / Application.targetFrameRate;
            float timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;

            TaskResult<Optional<GameObject>> entityResult = new();
            TaskResult<Exception> exception = new();

            while (batch.Count > 0)
            {
                entityResult.Set(Optional.Empty);
                exception.Set(null);

                Entity entity = batch[^1];
                batch.RemoveAt(batch.Count - 1);

                // Preconditions which may get the spawn process cancelled or postponed
                if (deletedEntitiesIds.Remove(entity.Id))
                {
                    continue;
                }
                if (WasAlreadySpawned(entity) && !forceRespawn)
                {
                    UpdateEntity(entity);
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

                entityMetadataManager.ApplyMetadata(entityResult.Get().Value, entity.Metadata);

                MarkAsSpawned(entity);

                // Finding out about all children (can be hidden in the object's hierarchy or in a pending list)
                
                if (!entitySpawner.SpawnsOwnChildren(entity))
                {
                    batch.AddRange(entity.ChildEntities);

                    List<NitroxId> childrenIds = entity.ChildEntities.Select(entity => entity.Id).ToList();
                    if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
                    {
                        IEnumerable<Entity> childrenToAdd = pendingEntities.Where(e => !childrenIds.Contains(e.Id));
                        batch.AddRange(childrenToAdd);
                        pendingParentEntitiesByParentId.Remove(entity.Id);
                    }
                }
                
                // Skip a frame to maintain FPS
                if (Time.realtimeSinceStartup >= timeLimit && skipFrames)
                {
                    yield return new WaitForEndOfFrame();
                    timeLimit = Time.realtimeSinceStartup + allottedTimePerFrame;
                }
            }
        }

        public IEnumerator SpawnEntityAsync(Entity entity, bool forceRespawn = false, bool skipFrames = false)
        {
            return SpawnBatchAsync(new() { entity }, forceRespawn, skipFrames);
        }

        public void CleanupExistingEntities(List<Entity> dirtyEntities)
        {
            foreach (Entity entity in dirtyEntities)
            {
                RemoveEntityHierarchy(entity);

                Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(entity.Id);

                if (gameObject.HasValue)
                {
                    UnityEngine.Object.Destroy(gameObject.Value);
                }
            }
        }

        private void UpdateEntity(Entity entity)
        {
            if (!NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject))
            {
#if DEBUG && ENTITY_LOG
                Log.Error($"Entity was already spawned but not found(is it in another chunk?) NitroxId: {entity.Id} TechType: {entity.TechType} ClassId: {entity.ClassId} Transform: {entity.Transform}");
#endif
                return;
            }
            entityMetadataManager.ApplyMetadata(gameObject, entity.Metadata);
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
                return type == entity.GetType() && NitroxEntity.TryGetObjectFrom(entity.Id, out _);
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
            return WasParentSpawned(id) || NitroxEntity.TryGetObjectFrom(id, out GameObject _);
        }

        public bool WasParentSpawned(NitroxId id)
        {
            return spawnedAsType.ContainsKey(id);
        }

        public void MarkAsSpawned(Entity entity)
        {
            spawnedAsType[entity.Id] = entity.GetType();
        }

        public void RemoveEntity(NitroxId id) => spawnedAsType.Remove(id);

        public void MarkForDeletion(NitroxId id)
        {
            deletedEntitiesIds.Add(id);
        }

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
