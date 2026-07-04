using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Serialization;
using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.GameLogic.Players;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Serialization.World;

internal class WorldService : IHostedService
{
    private readonly BatchEntitySpawner batchEntitySpawner;
    private readonly EntityRegistry entityRegistry;
    private readonly EscapePodManager escapePodManager;
    private readonly ServerJsonSerializer jsonSerializer;
    private readonly ILogger<WorldService> logger;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PdaManager pdaManager;
    private readonly PlayerManager playerManager;

    private readonly SubnauticaServerProtoBufSerializer protoBufSerializer;
    private readonly SaveService saveService;
    private readonly IOptions<ServerStartOptions> startOptions;
    private readonly StoryManager storyManager;
    private readonly StoryScheduler storyScheduler;
    private readonly TimeService timeService;
    private readonly SaveDataUpgrade[] upgrades;
    private readonly WorldEntityManager worldEntityManager;
    public IServerSerializer Serializer { get; private set; }
    private string FileEnding => Serializer?.FileEnding ?? "";

    private const string LEGACY_MAP_ROOM_CAMERA_CLASS_ID = "MapRoomCamera";
    private const string MAP_ROOM_CAMERA_PREFAB_CLASS_ID = "733fd479-0760-4bc2-a03e-281cbf02bfa4";

    public WorldService(
        SubnauticaServerProtoBufSerializer protoBufSerializer,
        ServerJsonSerializer jsonSerializer,
        IOptions<SubnauticaServerOptions> options,
        IEnumerable<SaveDataUpgrade> upgrades,
        BatchEntitySpawner batchEntitySpawner,
        EntityRegistry entityRegistry,
        PlayerManager playerManager,
        StoryScheduler storyScheduler,
        StoryManager storyManager,
        WorldEntityManager worldEntityManager,
        TimeService timeService,
        PdaManager pdaManager,
        EscapePodManager escapePodManager,
        SaveService saveService,
        IOptions<ServerStartOptions> startOptions,
        ILogger<WorldService> logger)
    {
        this.protoBufSerializer = protoBufSerializer;
        this.jsonSerializer = jsonSerializer;
        this.options = options;
        this.upgrades = upgrades.ToArray();
        this.batchEntitySpawner = batchEntitySpawner;
        this.entityRegistry = entityRegistry;
        this.playerManager = playerManager;
        this.storyScheduler = storyScheduler;
        this.storyManager = storyManager;
        this.worldEntityManager = worldEntityManager;
        this.timeService = timeService;
        this.pdaManager = pdaManager;
        this.escapePodManager = escapePodManager;
        this.saveService = saveService;
        this.startOptions = startOptions;
        this.logger = logger;

        UpdateSerializer(options.Value.SerializerMode);
    }

    public bool Save(string saveDir)
    {
        PersistedWorldData persistedWorld = new()
        {
            WorldData = new()
            {
                ParsedBatchCells = batchEntitySpawner.SerializableParsedBatches,
                GameData = GameData.From(pdaManager, storyManager.StoryGoalData, storyScheduler, storyManager, timeService)
            },
            PlayerData = PlayerData.From(playerManager.GetAllPlayers()),
            GlobalRootData = GlobalRootData.From(worldEntityManager.GetPersistentGlobalRootEntities()),
            EntityData = EntityData.From(entityRegistry.GetAllEntities(true))
        };
        return Save(persistedWorld, saveDir);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await LoadWorldFromSavePathAsync(startOptions.Value.GetServerSavePath()))
        {
            await CreateAndLoadWorldAsync();
        }
        await CreateFullEntityCacheIfRequested();
        return;

        async Task CreateFullEntityCacheIfRequested()
        {
            if (!options.Value.CreateFullEntityCache)
            {
                return;
            }

            try
            {
                logger.ZLogInformation($"Starting to load all batches up front.");
                logger.ZLogInformation($"This can take up to several minutes and you can't join until it's completed.");
                logger.ZLogInformation($"{entityRegistry.GetAllEntities().Count} entities already cached");
                if (entityRegistry.GetAllEntities().Count < 504732)
                {
                    await worldEntityManager.LoadAllUnspawnedEntitiesAsync(cancellationToken);

                    logger.ZLogInformation($"Saving newly cached entities.");
                    await saveService.QueueActionAsync(SaveService.ServiceAction.SAVE, cancellationToken);
                }
                logger.ZLogInformation($"All batches have now been loaded.");
            }
            catch (OperationCanceledException ex)
            {
                logger.ZLogWarning($"Server start was cancelled by user:{Environment.NewLine}{ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    internal void UpdateSerializer(IServerSerializer serverSerializer)
    {
        Validate.NotNull(serverSerializer, "Serializer cannot be null");
        Serializer = serverSerializer;
    }

    internal void UpdateSerializer(ServerSerializerMode mode) => Serializer = mode == ServerSerializerMode.PROTOBUF ? protoBufSerializer : jsonSerializer;

    internal bool Save(PersistedWorldData persistedData, string saveDir)
    {
        try
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            Serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
            Serializer.Serialize(Path.Combine(saveDir, $"PlayerData{FileEnding}"), persistedData.PlayerData);
            Serializer.Serialize(Path.Combine(saveDir, $"WorldData{FileEnding}"), persistedData.WorldData);
            Serializer.Serialize(Path.Combine(saveDir, $"GlobalRootData{FileEnding}"), persistedData.GlobalRootData);
            Serializer.Serialize(Path.Combine(saveDir, $"EntityData{FileEnding}"), persistedData.EntityData);

            logger.ZLogInformation($"World state saved");
            return true;
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Could not save world :");
            return false;
        }
    }

    internal PersistedWorldData? LoadPersistedWorld(string saveDir)
    {
        try
        {
            PersistedWorldData persistedData = new()
            {
                PlayerData = Serializer.Deserialize<PlayerData>(Path.Combine(saveDir, $"PlayerData{FileEnding}")),
                WorldData = Serializer.Deserialize<WorldData>(Path.Combine(saveDir, $"WorldData{FileEnding}")),
                GlobalRootData = Serializer.Deserialize<GlobalRootData>(Path.Combine(saveDir, $"GlobalRootData{FileEnding}")),
                EntityData = Serializer.Deserialize<EntityData>(Path.Combine(saveDir, $"EntityData{FileEnding}"))
            };
            EntityDataPostProcessing(persistedData.EntityData);
            GlobalRootDataPostProcessing(persistedData.GlobalRootData);

            if (!persistedData.IsValid())
            {
                throw new InvalidDataException("Save files are not valid");
            }

            return persistedData;
        }
        catch (Exception ex)
        {
            // Check if the world was newly created using the world manager
            if (new FileInfo(Path.Combine(saveDir, $"Version{FileEnding}")).Length > 0)
            {
                // Give error saying that world could not be used, and to restore a backup
                logger.ZLogError($"Could not load world, please restore one of your backups to continue using this world. : {ex.GetType()} {ex.Message}");

                throw;
            }
        }

        return null;

        void EntityDataPostProcessing(EntityData entityData)
        {
            // After deserialization, we want to assign all of the
            // children to their respective parent entities.
            Dictionary<NitroxId, Entity> entitiesById = entityData.Entities.ToDictionary(entity => entity.Id);

            bool brokenEntitiesFound = false;

            foreach (Entity entity in entityData.Entities)
            {
                if (entity is WorldEntity we)
                {
                    NitroxVector3 pos = we.Transform.LocalPosition;
                    if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z) ||
                        float.IsInfinity(pos.X) || float.IsInfinity(pos.Y) || float.IsInfinity(pos.Z))
                    {
                        logger.ZLogError($"Found {nameof(WorldEntity)} with NaN or infinite position. Teleporting it to world origin.");
                        we.Transform.LocalPosition = NitroxVector3.Zero;
                    }

                    NitroxQuaternion rot = we.Transform.LocalRotation;
                    if (float.IsNaN(rot.X) || float.IsNaN(rot.Y) || float.IsNaN(rot.Z) || float.IsNaN(rot.W) ||
                        float.IsInfinity(rot.X) || float.IsInfinity(rot.Y) || float.IsInfinity(rot.Z) || float.IsInfinity(rot.W))
                    {
                        logger.ZLogError($"Found {nameof(WorldEntity)} with NaN or infinite rotation. Resetting rotation.");
                        we.Transform.LocalRotation = NitroxQuaternion.Identity;
                    }

                    if (we.Level == 100)
                    {
                        logger.ZLogError($"Found a world entity with cell level 100. It will be hotfixed.\n{we}");
                        we.Level = 0;
                        brokenEntitiesFound = true;
                    }
                }

                // We will re-build the child hierarchy below and want to avoid duplicates.
                // TODO: Rework system to no longer persist children entities because they are duplicates.
                entity.ChildEntities.Clear();
            }

            if (brokenEntitiesFound)
            {
                logger.ZLogWarningOnce($"Please consider reporting the above issues on the Discord server.");
            }

            foreach (Entity entity in entityData.Entities)
            {
                if (entity.ParentId != null && entitiesById.TryGetValue(entity.ParentId, out Entity parentEntity))
                {
                    parentEntity.ChildEntities.Add(entity);

                    if (entity is WorldEntity worldEntity && parentEntity is WorldEntity parentWorldEntity)
                    {
                        worldEntity.Transform.SetParent(parentWorldEntity.Transform, false);
                    }
                }
            }
        }

        void GlobalRootDataPostProcessing(GlobalRootData globalRootData)
        {
            foreach (GlobalRootEntity entity in globalRootData.Entities)
            {
                EnsureChildrenTransformAreParented(entity);
            }

            void EnsureChildrenTransformAreParented(WorldEntity entity)
            {
                if (entity.Transform == null)
                {
                    return;
                }

                bool brokenEntitiesFound = false;

                foreach (Entity child in entity.ChildEntities)
                {
                    if (child is not WorldEntity childWe)
                    {
                        continue;
                    }

                    if (child is not GlobalRootEntity && childWe.Level == 100)
                    {
                        logger.ZLogError($"Found a world entity with cell level 100. It will be hotfixed.\n{childWe}");
                        childWe.Level = 0;
                        brokenEntitiesFound = true;
                    }

                    if (childWe.Transform != null)
                    {
                        childWe.Transform.SetParent(entity.Transform, false);
                        EnsureChildrenTransformAreParented(childWe);
                    }
                }

                if (brokenEntitiesFound)
                {
                    logger.ZLogWarningOnce($"Please consider reporting the above issues on the Discord server.");
                }
            }
        }
    }

    // TODO: This method should be removed. Each service should load its own data instead of centralizing it here.
    private async Task LoadPersistedWorldIntoServicesAsync(PersistedWorldData pWorldData)
    {
        string seed = options.Value.Seed;
        logger.ZLogInformation($"Loading world with seed {seed}");

        // Time
        timeService.ActiveTime = TimeSpan.FromSeconds(pWorldData.WorldData.GameData.StoryTiming.RealTimeElapsed);
        // Entities
        // Entities
        // Entities
        NormalizeLooseMapRoomCameraWorldEntities(pWorldData);
        PurgeLegacyBrokenMapRoomCameraEntities(pWorldData);

        entityRegistry.AddEntities(pWorldData.EntityData.Entities);
        entityRegistry.AddEntitiesIgnoringDuplicate(pWorldData.GlobalRootData.Entities.OfType<Entity>().ToList());

        SanitizeLegacyMapRoomCameraDockingState(entityRegistry);
        MapRoomCameraRegistry.LoadFromSave(pWorldData.WorldData.GameData?.MapRoomCameraRegistry, entityRegistry);

        await escapePodManager.AddKnownPodsAsync(entityRegistry.GetEntities<EscapePodEntity>());

        // TODO: hacky code - see WorldEntityManager for more information.
        List<WorldEntity> worldEntities = entityRegistry.GetEntities<WorldEntity>();
        worldEntityManager.globalRootEntitiesById = entityRegistry.GetEntities<GlobalRootEntity>().ToDictionary(entity => entity.Id);
        worldEntityManager.worldEntitiesByCell = worldEntities.Where(entity => entity is not GlobalRootEntity)
                                                              .GroupBy(entity => entity.AbsoluteEntityCell)
                                                              .ToDictionary(group => group.Key, group => group.ToDictionary(entity => entity.Id, entity => entity));

        foreach (Player player in pWorldData.PlayerData.GetPlayers())
        {
            playerManager.AddSavedPlayer(player);
        }
        batchEntitySpawner.SerializableParsedBatches = pWorldData.WorldData.ParsedBatchCells;
        // Pda
        pdaManager.PdaState = pWorldData.WorldData.GameData.PDAState;
        // Story progression
        storyManager.StoryGoalData = pWorldData.WorldData.GameData.StoryGoals;
        storyScheduler.ScheduleStoriesIfNotInPast(pWorldData.WorldData.GameData.StoryGoals.ScheduledGoals);
        // Global story timed events
        storyManager.AuroraCountdownTimeMs = pWorldData.WorldData.GameData.StoryTiming.AuroraCountdownTime ?? storyManager.GenerateDeterministicAuroraTime(seed);
        storyManager.AuroraWarningTimeMs = pWorldData.WorldData.GameData.StoryTiming.AuroraWarningTime ?? timeService.GameTime.TotalMilliseconds;
        // +27 is from CrashedShipExploder.IsExploded, -480 is from the default time (see TimeService)
        storyManager.AuroraRealExplosionTime = TimeSpan.FromSeconds(pWorldData.WorldData.GameData.StoryTiming.AuroraRealExplosionTime ?? storyManager.AuroraCountdownTimeMs * 0.001 + 27 - TimeService.DEFAULT_STARTING_GAME_TIME_SECONDS);

        logger.ZLogInformation($"World finished loading");
    }

    private void NormalizeLooseMapRoomCameraWorldEntities(PersistedWorldData pWorldData)
    {
        List<WorldEntity> looseCameraEntities = pWorldData.EntityData.Entities
                                                              .Where(entity => entity != null)
                                                              .Cast<Entity>()
                                                              .Concat(pWorldData.GlobalRootData.Entities
                                                                                          .Where(entity => entity != null)
                                                                                          .Cast<Entity>())
                                                              .OfType<WorldEntity>()
                                                              .Where(entity => entity.ParentId == null)
                                                              .Where(IsLooseMapRoomCameraWorldEntity)
                                                              .ToList();

        foreach (WorldEntity worldEntity in looseCameraEntities)
        {
            string oldClassId = worldEntity.ClassId ?? "";
            string oldTechType = worldEntity.TechType?.ToString() ?? "";
            int oldLevel = worldEntity.Level;

            worldEntity.ClassId = MAP_ROOM_CAMERA_PREFAB_CLASS_ID;
            worldEntity.TechType = TechType.MapRoomCamera.ToDto();

            if (worldEntity is GlobalRootEntity)
            {
                worldEntity.Level = GlobalRootEntity.GLOBAL_ROOT_LEVEL;
            }

            logger.ZLogWarning($"Normalized loose scanner room camera world entity: id={worldEntity.Id}, oldClassId={oldClassId}, newClassId={worldEntity.ClassId}, oldTechType={oldTechType}, newTechType={worldEntity.TechType}, oldLevel={oldLevel}, newLevel={worldEntity.Level}, entityType={worldEntity.GetType().Name}, position={worldEntity.Transform?.Position}");
        }
    }

    private static bool IsLooseMapRoomCameraWorldEntity(WorldEntity worldEntity)
    {
        string techType = worldEntity.TechType?.ToString() ?? "";
        string classId = worldEntity.ClassId ?? "";
        string entityText = worldEntity.ToString();

        return techType == "MapRoomCamera" ||
               classId == LEGACY_MAP_ROOM_CAMERA_CLASS_ID ||
               classId == MAP_ROOM_CAMERA_PREFAB_CLASS_ID ||
               classId.Contains("MapRoomCamera") ||
               entityText.Contains("MapRoomCamera");
    }

    private void PurgeLegacyBrokenMapRoomCameraEntities(PersistedWorldData pWorldData)
    {
        HashSet<NitroxId> validDockedCameraIds = pWorldData.GlobalRootData.Entities
                                                           .OfType<MapRoomEntity>()
                                                           .Where(mapRoomEntity => mapRoomEntity.CameraDockingIds != null)
                                                           .SelectMany(mapRoomEntity => mapRoomEntity.CameraDockingIds)
                                                           .Where(cameraId => cameraId != null)
                                                           .ToHashSet();

        HashSet<NitroxId> validRegistryCameraIds = pWorldData.WorldData.GameData?.MapRoomCameraRegistry?
                                                            .Where(entry => entry.CameraId != null)
                                                            .Select(entry => entry.CameraId)
                                                            .ToHashSet() ?? [];

        List<Entity> allEntities = pWorldData.EntityData.Entities
                                     .Where(entity => entity != null)
                                     .Cast<Entity>()
                                     .Concat(pWorldData.GlobalRootData.Entities
                                                           .Where(entity => entity != null)
                                                           .Cast<Entity>())
                                     .ToList();

        HashSet<NitroxId> validLooseCameraIds = allEntities
                                                .OfType<WorldEntity>()
                                                .Where(entity => entity.Id != null)
                                                .Where(entity => entity.ParentId == null)
                                                .Where(IsLooseMapRoomCameraWorldEntity)
                                                .Select(entity => entity.Id)
                                                .ToHashSet();

        HashSet<NitroxId> validCameraIds = validDockedCameraIds;
        validCameraIds.UnionWith(validRegistryCameraIds);
        validCameraIds.UnionWith(validLooseCameraIds);

        Dictionary<NitroxId, Entity> allEntitiesById = allEntities
                                                       .Where(entity => entity.Id != null)
                                                       .GroupBy(entity => entity.Id)
                                                       .ToDictionary(group => group.Key, group => group.First());

        HashSet<NitroxId> batteryParentIds = allEntities
                                             .Where(IsBatteryChildEntity)
                                             .Where(entity => entity.ParentId != null)
                                             .Select(entity => entity.ParentId)
                                             .ToHashSet();

        HashSet<NitroxId> legacyCameraIdsToRemove = allEntities
                                                    .Where(entity => entity.Id != null)
                                                    .Where(entity => !validCameraIds.Contains(entity.Id))
                                                    .Where(entity => IsLegacyScannerCameraRootCandidate(entity, batteryParentIds))
                                                    .Select(entity => entity.Id)
                                                    .ToHashSet();

        // Extra safety: if the save has battery children whose parent is one of the known bad scanner-camera-like roots,
        // include the parent root too. This catches old camera roots whose TechType is null or not MapRoomCamera.
        foreach (NitroxId batteryParentId in batteryParentIds)
        {
            if (batteryParentId == null || validCameraIds.Contains(batteryParentId))
            {
                continue;
            }

            if (!allEntitiesById.TryGetValue(batteryParentId, out Entity parentEntity))
            {
                continue;
            }

            if (IsLegacyScannerCameraRootCandidate(parentEntity, batteryParentIds))
            {
                legacyCameraIdsToRemove.Add(batteryParentId);
            }
        }

        if (legacyCameraIdsToRemove.Count == 0)
        {
            logger.ZLogWarning($"No legacy broken scanner room camera root entities matched purge. Battery parents=[{string.Join(", ", batteryParentIds)}], validCameraIds=[{string.Join(", ", validCameraIds)}]");
            return;
        }

        int removedEntityCount = RemoveEntitiesAndChildren(pWorldData.EntityData.Entities, legacyCameraIdsToRemove);
        int removedGlobalRootCount = RemoveEntitiesAndChildren(pWorldData.GlobalRootData.Entities, legacyCameraIdsToRemove);

        pWorldData.WorldData.GameData?.MapRoomCameraRegistry?.RemoveAll(entry => entry.CameraId != null && legacyCameraIdsToRemove.Contains(entry.CameraId));

        logger.ZLogWarning($"Purged legacy broken scanner room camera roots [{string.Join(", ", legacyCameraIdsToRemove)}] and {removedEntityCount + removedGlobalRootCount} total related entities from saved world data.");
    }

    private static bool IsLegacyScannerCameraRootCandidate(Entity entity, HashSet<NitroxId> batteryParentIds)
    {
        if (entity is not WorldEntity worldEntity)
        {
            return false;
        }

        if (worldEntity.ParentId != null)
        {
            return false;
        }

        if (!batteryParentIds.Contains(worldEntity.Id))
        {
            return false;
        }

        string techType = worldEntity.TechType?.ToString() ?? "";
        string classId = worldEntity.ClassId ?? "";
        string entityText = worldEntity.ToString();

        return techType == "MapRoomCamera" ||
               classId.Contains("MapRoomCamera") ||
               entityText.Contains("MapRoomCamera");
    }

    private static bool IsBatteryChildEntity(Entity entity)
    {
        return entity is PrefabChildEntity childEntity &&
               childEntity.TechType.ToString() == "Battery";
    }

    private static int RemoveEntitiesAndChildren<T>(List<T> entities, HashSet<NitroxId> rootIdsToRemove) where T : Entity
    {
        HashSet<NitroxId> idsToRemove = new(rootIdsToRemove);
        bool foundNewChild;

        do
        {
            foundNewChild = false;

            foreach (Entity entity in entities.Where(entity => entity != null))
            {
                if (entity.ParentId != null &&
                    entity.Id != null &&
                    idsToRemove.Contains(entity.ParentId) &&
                    idsToRemove.Add(entity.Id))
                {
                    foundNewChild = true;
                }
            }
        }
        while (foundNewChild);

        return entities.RemoveAll(entity => entity?.Id != null && idsToRemove.Contains(entity.Id));
    }

    private void SanitizeLegacyMapRoomCameraDockingState(EntityRegistry entityRegistry)
    {
        foreach (MapRoomEntity mapRoomEntity in entityRegistry.GetEntities<MapRoomEntity>())
        {
            if (mapRoomEntity.CameraDockingStates == null)
            {
                continue;
            }

            mapRoomEntity.CameraDockingIds ??= [];

            while (mapRoomEntity.CameraDockingIds.Count < mapRoomEntity.CameraDockingStates.Count)
            {
                mapRoomEntity.CameraDockingIds.Add(null);
            }

            for (int i = 0; i < mapRoomEntity.CameraDockingStates.Count; i++)
            {
                if (mapRoomEntity.CameraDockingStates[i] && mapRoomEntity.CameraDockingIds[i] == null)
                {
                    logger.ZLogWarning($"Clearing legacy scanner room docking state with no camera id. mapRoom={mapRoomEntity.Id}, dockingIndex={i}");
                    mapRoomEntity.CameraDockingStates[i] = false;
                }
            }
        }
    }

    private async Task<bool> LoadWorldFromSavePathAsync(string saveDir)
    {
        if (!Directory.Exists(saveDir) || !File.Exists(Path.Combine(saveDir, $"Version{FileEnding}")))
        {
            logger.ZLogWarning($"No previous save file found, creating a new one");
            return false;
        }

        UpgradeSave(saveDir);

        PersistedWorldData persistedData = LoadPersistedWorld(saveDir);
        if (persistedData == null)
        {
            return false;
        }
        await LoadPersistedWorldIntoServicesAsync(persistedData);
        return true;
    }

    private async Task CreateAndLoadWorldAsync()
    {
        PersistedWorldData pWorldData = new()
        {
            EntityData = EntityData.From([]),
            PlayerData = PlayerData.From([]),
            WorldData = new WorldData
            {
                GameData = new GameData
                {
                    PDAState = new PdaStateData(),
                    StoryGoals = new StoryGoalData(),
                    StoryTiming = new StoryTimingData()
                },
                ParsedBatchCells = []
            },
            GlobalRootData = new GlobalRootData()
        };
        await LoadPersistedWorldIntoServicesAsync(pWorldData);
        InitNewWorld();

        void InitNewWorld()
        {
            // Creating entities for the 11 RadiationLeakPoint located at (Aurora Scene) //Aurora-MainPrefab/Aurora/radiationleaks/RadiationLeaks(Clone)
            const int TOTAL_LEAKS = 11; // This constant is defined by Subnautica and should never be modified
            for (int i = 0; i < TOTAL_LEAKS; i++)
            {
                RadiationLeakEntity leakEntity = new(new(), i, new(0));
                worldEntityManager.AddOrUpdateGlobalRootEntity(leakEntity);
            }
        }
    }

    private void UpgradeSave(string saveDir)
    {
        SaveFileVersion saveFileVersion;

        try
        {
            saveFileVersion = Serializer.Deserialize<SaveFileVersion>(Path.Combine(saveDir, $"Version{FileEnding}"));
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error while upgrading save file. \"Version{FileEnding}\" couldn't be read.");
            return;
        }

        if (saveFileVersion == null || saveFileVersion.Version == NitroxEnvironment.Version)
        {
            return;
        }

        if (options.Value.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            logger.ZLogInformation($"Can't upgrade while using ProtoBuf as serializer");
        }
        else
        {
            try
            {
                foreach (SaveDataUpgrade upgrade in upgrades)
                {
                    if (upgrade.TargetVersion > saveFileVersion.Version)
                    {
                        upgrade.UpgradeSaveFiles(saveDir, FileEnding);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Error while upgrading save file.");
                return;
            }

            Serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
            logger.ZLogInformation($"Save file was upgraded to {NitroxEnvironment.Version:@Version}");
        }
    }
}
