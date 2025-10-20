using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
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
    private readonly ServerJsonSerializer jsonSerializer;
    private readonly ILogger<WorldService> logger;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PlayerManager playerManager;

    private readonly SubnauticaServerProtoBufSerializer protoBufSerializer;
    private readonly ScheduleKeeper scheduleKeeper;
    private readonly IOptions<ServerStartOptions> startOptions;
    private readonly StoryManager storyManager;
    private readonly TimeService timeService;
    private readonly PdaStateData pdaState;
    private readonly StoryGoalData storyGoals;
    private readonly EscapePodManager escapePodManager;
    private readonly SubnauticaResourceLoaderService resourceLoaderService;
    private readonly SaveDataUpgrade[] upgrades;
    private readonly WorldEntityManager worldEntityManager;
    public IServerSerializer Serializer { get; private set; }
    private string FileEnding => Serializer?.FileEnding ?? "";

    public WorldService(
        SubnauticaServerProtoBufSerializer protoBufSerializer,
        ServerJsonSerializer jsonSerializer,
        IOptions<SubnauticaServerOptions> options,
        IEnumerable<SaveDataUpgrade> upgrades,
        BatchEntitySpawner batchEntitySpawner,
        EntityRegistry entityRegistry,
        PlayerManager playerManager,
        ScheduleKeeper scheduleKeeper,
        StoryManager storyManager,
        WorldEntityManager worldEntityManager,
        TimeService timeService,
        PdaStateData pdaState,
        StoryGoalData storyGoals,
        EscapePodManager escapePodManager,
        SubnauticaResourceLoaderService resourceLoaderService,
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
        this.scheduleKeeper = scheduleKeeper;
        this.storyManager = storyManager;
        this.worldEntityManager = worldEntityManager;
        this.timeService = timeService;
        this.pdaState = pdaState;
        this.storyGoals = storyGoals;
        this.escapePodManager = escapePodManager;
        this.resourceLoaderService = resourceLoaderService;
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
                GameData = GameData.From(pdaState, storyGoals, scheduleKeeper, storyManager, timeService),
                Seed = options.Value.Seed,
            },
            PlayerData = PlayerData.From(playerManager.GetAllPlayers()),
            GlobalRootData = GlobalRootData.From(worldEntityManager.GetPersistentGlobalRootEntities()),
            EntityData = EntityData.From(entityRegistry.GetAllEntities(true))
        };
        return Save(persistedWorld, saveDir);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await resourceLoaderService.AllResourcesLoadedTask;
        if (!LoadWorldFromSavePath(startOptions.Value.GetServerSavePath()))
        {
            CreateAndLoadWorld();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void LoadPersistedWorldIntoServices(PersistedWorldData pWorldData)
    {
        string seed = pWorldData.WorldData.Seed ?? options.Value.Seed ?? throw new InvalidOperationException("World seed must not be null");
        // Initialized only once, just like UnityEngine.Random
        XorRandom.InitSeed(seed.GetHashCode());

        logger.ZLogInformation($"Loading world with seed {seed}");

        entityRegistry.AddEntities(pWorldData.EntityData.Entities);
        entityRegistry.AddEntitiesIgnoringDuplicate(pWorldData.GlobalRootData.Entities.OfType<Entity>().ToList());
        escapePodManager.AddKnownPods(entityRegistry.GetEntities<EscapePodEntity>());
        foreach (Player player in pWorldData.PlayerData.GetPlayers())
        {
            playerManager.AddPlayer(player);
        }
        timeService.RealTime = TimeSpan.FromMilliseconds(pWorldData.WorldData.GameData.StoryTiming.RealTimeElapsed);
        timeService.GameTime = TimeSpan.FromSeconds(pWorldData.WorldData.GameData.StoryTiming.ElapsedSeconds);
        batchEntitySpawner.SerializableParsedBatches = pWorldData.WorldData.ParsedBatchCells;

        // Pda data
        pdaState.AnalyzedTechTypes.AddRange(pWorldData.WorldData.GameData.PDAState.AnalyzedTechTypes);
        pdaState.EncyclopediaEntries.AddRange(pWorldData.WorldData.GameData.PDAState.EncyclopediaEntries);
        pdaState.KnownTechTypes.AddRange(pWorldData.WorldData.GameData.PDAState.KnownTechTypes);
        pdaState.PdaLog.AddRange(pWorldData.WorldData.GameData.PDAState.PdaLog);
        pdaState.ScannerComplete.AddRange(pWorldData.WorldData.GameData.PDAState.ScannerComplete);
        foreach (NitroxId fragment in pWorldData.WorldData.GameData.PDAState.ScannerFragments)
        {
            pdaState.ScannerFragments.Add(fragment);
        }
        pdaState.ScannerPartial.AddRange(pWorldData.WorldData.GameData.PDAState.ScannerPartial);

        // Story progression data
        foreach (string goal in pWorldData.WorldData.GameData.StoryGoals.CompletedGoals)
        {
            storyGoals.CompletedGoals.Add(goal);
        }
        foreach (string radioItem in pWorldData.WorldData.GameData.StoryGoals.RadioQueue.ToArray().Reverse())
        {
            storyGoals.RadioQueue.Enqueue(radioItem);
        }
        storyGoals.ScheduledGoals.AddRange(pWorldData.WorldData.GameData.StoryGoals.ScheduledGoals);

        // Story timed events data
        storyManager.AuroraCountdownTimeMs = pWorldData.WorldData.GameData.StoryTiming.AuroraCountdownTime ?? storyManager.GenerateDeterministicAuroraTime(seed);
        storyManager.AuroraWarningTimeMs = pWorldData.WorldData.GameData.StoryTiming.AuroraWarningTime ?? timeService.GameTime.TotalMilliseconds;
        // +27 is from CrashedShipExploder.IsExploded, -480 is from the default time (see TimeKeeper)
        storyManager.AuroraRealExplosionTime = TimeSpan.FromSeconds(pWorldData.WorldData.GameData.StoryTiming.AuroraRealExplosionTime ?? storyManager.AuroraCountdownTimeMs * 0.001 + 27 - TimeService.DEFAULT_STARTING_GAME_TIME_SECONDS);

        logger.ZLogInformation($"World finished loading");
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

            options.Value.Seed = persistedData.WorldData.Seed;

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
    }

    private bool LoadWorldFromSavePath(string saveDir)
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
        LoadPersistedWorldIntoServices(persistedData);
        return true;
    }

    private void CreateAndLoadWorld()
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
                ParsedBatchCells = [],
                Seed = options.Value.Seed
            },
            GlobalRootData = new GlobalRootData()
        };
        LoadPersistedWorldIntoServices(pWorldData);

        // This constant is defined by Subnautica and should never be modified
        const int TOTAL_LEAKS = 11;
        // Creating entities for the 11 RadiationLeakPoint located at (Aurora Scene) //Aurora-MainPrefab/Aurora/radiationleaks/RadiationLeaks(Clone)
        for (int i = 0; i < TOTAL_LEAKS; i++)
        {
            RadiationLeakEntity leakEntity = new(new(), i, new(0));
            worldEntityManager.AddOrUpdateGlobalRootEntity(leakEntity);
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
