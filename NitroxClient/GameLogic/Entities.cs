using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;
        private readonly EntityMetadataManager entityMetadataManager;
        private readonly SimulationOwnership simulationOwnership;
        private readonly Terrain terrain;

        private readonly Dictionary<NitroxId, Type> spawnedAsType = [];

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = [];

        public List<Entity> EntitiesToSpawn { get; private init; }
        public List<AbsoluteEntityCell> CellsToSpawn { get; private init; }
        public bool SpawningEntities { get; private set; }

        private readonly HashSet<NitroxId> deletedEntitiesIds = [];

        /// <remarks>
        /// We divide the FPS by 2.5 because we consider (time for 1 frame + spawning time without a frame + extra computing time).
        /// </remarks>
        private static float allottedTimePerFrame => 0.4f / Application.targetFrameRate;
        private readonly TaskResult<Optional<GameObject>> entityResult = new();
        private readonly TaskResult<Exception> exception = new();
        private float timeUntilNextYield;
        public bool ShouldSkipFrame => Time.realtimeSinceStartup >= timeUntilNextYield;


        public Entities(IPacketSender packetSender, ThrottledPacketSender throttledPacketSender, EntityMetadataManager entityMetadataManager, PlayerManager playerManager, LocalPlayer localPlayer, LiveMixinManager liveMixinManager, TimeManager timeManager, SimulationOwnership simulationOwnership, Terrain terrain)
        {
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;
            this.entityMetadataManager = entityMetadataManager;
            this.simulationOwnership = simulationOwnership;
            this.terrain = terrain;

            EntitiesToSpawn = [];
            CellsToSpawn = [];

            entitySpawnersByType[typeof(PrefabChildEntity)] = new PrefabChildEntitySpawner();
            entitySpawnersByType[typeof(PathBasedChildEntity)] = new PathBasedChildEntitySpawner();
            entitySpawnersByType[typeof(InstalledModuleEntity)] = new InstalledModuleEntitySpawner();
            entitySpawnersByType[typeof(InstalledBatteryEntity)] = new InstalledBatteryEntitySpawner();
            entitySpawnersByType[typeof(InventoryEntity)] = new InventoryEntitySpawner();
            entitySpawnersByType[typeof(InventoryItemEntity)] = new InventoryItemEntitySpawner(entityMetadataManager);
            entitySpawnersByType[typeof(WorldEntity)] = new WorldEntitySpawner(entityMetadataManager, this, simulationOwnership);
            entitySpawnersByType[typeof(PlaceholderGroupWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(PrefabPlaceholderEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(EscapePodEntity)] = new EscapePodEntitySpawner(localPlayer);
            entitySpawnersByType[typeof(PlayerEntity)] = new PlayerEntitySpawner(playerManager, localPlayer, this);
            entitySpawnersByType[typeof(VehicleEntity)] = new VehicleEntitySpawner();
            entitySpawnersByType[typeof(SerializedWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(GlobalRootEntity)] = new GlobalRootEntitySpawner();
            entitySpawnersByType[typeof(BaseLeakEntity)] = new BaseLeakEntitySpawner(liveMixinManager);
            entitySpawnersByType[typeof(BuildEntity)] = new BuildEntitySpawner(this, (BaseLeakEntitySpawner)entitySpawnersByType[typeof(BaseLeakEntity)]);
            entitySpawnersByType[typeof(RadiationLeakEntity)] = new RadiationLeakEntitySpawner(timeManager);
            entitySpawnersByType[typeof(ModuleEntity)] = new ModuleEntitySpawner(this);
            entitySpawnersByType[typeof(GhostEntity)] = new GhostEntitySpawner();
            entitySpawnersByType[typeof(OxygenPipeEntity)] = new OxygenPipeEntitySpawner((WorldEntitySpawner)entitySpawnersByType[typeof(WorldEntity)]);
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

        private IEnumerator SpawnNewEntities(bool coldStart = false)
        {
            if (coldStart)
            {
                yield return null; // Skips a frame
            }

            // The coroutine waits a frame after SpawnBatchAsync finishes, and another entity may be enqueued then, so a loop is needed
            while (EntitiesToSpawn.Count > 0)
            {
                yield return SpawnBatchAsync(EntitiesToSpawn).OnYieldError(Log.Error);
            }

            SpawningEntities = false;

            entityMetadataManager.ClearNewerMetadata();
            deletedEntitiesIds.Clear();
            simulationOwnership.ClearNewerSimulations();
            EntityPositionBroadcaster.Instance.ClearNotSpawnedEntities();
            
            foreach (AbsoluteEntityCell absoluteEntityCell in CellsToSpawn)
            {
                terrain.AddFullySpawnedCell(absoluteEntityCell);
            }
            CellsToSpawn.Clear();
        }

        public void EnqueueEntitiesToSpawn(List<Entity> entitiesToEnqueue, List<AbsoluteEntityCell> cellsToSpawn, bool coldStart = false)
        {
            EntitiesToSpawn.InsertRange(0, entitiesToEnqueue);
            CellsToSpawn.AddRange(cellsToSpawn);
            if (!SpawningEntities)
            {
                SpawningEntities = true;
                CoroutineHost.StartCoroutine(SpawnNewEntities(coldStart));
            }
        }

        public void RefreshTimeUntilNextYield()
        {
            timeUntilNextYield = Time.realtimeSinceStartup + allottedTimePerFrame;
        }

        /// <remarks>
        /// Yield returning takes too much time (at least once per IEnumerator branch) and it quickly gets out of hand with long function call hierarchies so
        /// we want to reduce the amount of yield operations and only skip to the next frame when required (to maintain the FPS).
        /// Also saves resources by using the IOut instances
        /// </remarks>
        /// <param name="batch"></param>
        /// <param name="forceRespawn">Should children be spawned even if already marked as spawned</param>
        /// <param name="skipFrames"></param>
        public IEnumerator SpawnBatchAsync(List<Entity> batch, bool forceRespawn = false, bool skipFrames = true)
        {
            RefreshTimeUntilNextYield();

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
                    continue;
                }
                else if (entity.ParentId != null && !IsParentReady(entity.ParentId))
                {
                    Log.ErrorOnce($"[{nameof(Entities)}] Could not find parent {entity.ParentId} when spawning {entity.Id}");
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

                OnEntitySpawned(entity, entityResult.Get().Value);
                simulationOwnership.ApplyNewerSimulation(entity.Id);

                if (!entitySpawner.SpawnsOwnChildren(entity))
                {
                    batch.AddRange(entity.ChildEntities);
                }

                // Skip a frame to maintain FPS
                if (ShouldSkipFrame && skipFrames)
                {
                    yield return new WaitForEndOfFrame();
                    RefreshTimeUntilNextYield();
                }
            }
        }

        public void OnEntitySpawned(Entity entity, GameObject gameObject)
        {
            entityMetadataManager.ApplyMetadata(entityResult.Get().Value, entity.Metadata);
            MarkAsSpawned(entity);
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
                    DestroyObject(gameObject.Value);
                }
            }
        }

        /// <summary>
        /// Either perform a special operation (e.g. for plants) or a simple <see cref="UnityEngine.Object.Destroy"/>
        /// </summary>
        public static void DestroyObject(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out Plantable plantable))
            {
                plantable.FreeSpot();
                return;
            }
            if (gameObject.TryGetComponent(out GrownPlant grownPlant))
            {
                grownPlant.seed.AliveOrNull()?.FreeSpot();
                return;
            }
            if (gameObject.TryGetComponent(out Pickupable pickupable))
            {
                // Running this now means it won't get ran in OnDestroy() at the end of the frame, so no packets get sent
                pickupable.SetInventoryItem(null);
            }

            UnityEngine.Object.Destroy(gameObject);
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
            return NitroxEntity.TryGetObjectFrom(id, out _);
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
