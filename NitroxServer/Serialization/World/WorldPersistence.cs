using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Server;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Resources;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;
        private readonly ServerConfig config;
        private readonly RandomStartGenerator randomStart;
        private readonly SaveDataUpgrade[] upgrades;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, ServerConfig config, RandomStartGenerator randomStart, SaveDataUpgrade[] upgrades)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
            this.config = config;
            this.randomStart = randomStart;
            this.upgrades = upgrades;

            UpdateSerializer(config.SerializerMode);
        }

        internal void UpdateSerializer(IServerSerializer serverSerializer)
        {
            Validate.NotNull(serverSerializer, "Serializer cannot be null");
            Serializer = serverSerializer;
        }

        internal void UpdateSerializer(ServerSerializerMode mode)
        {
            Serializer = (mode == ServerSerializerMode.PROTOBUF) ? protoBufSerializer : jsonSerializer;
        }

        public bool Save(World world, string saveDir) => Save(PersistedWorldData.From(world), saveDir);

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

                using (config.Update(saveDir))
                {
                    config.Seed = persistedData.WorldData.Seed;
                }

                Log.Info("World state saved");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Could not save world :");
                return false;
            }
        }

        internal Optional<World> LoadFromFile(string saveDir)
        {
            if (!Directory.Exists(saveDir) || !File.Exists(Path.Combine(saveDir, $"Version{FileEnding}")))
            {
                Log.Warn("No previous save file found, creating a new one");
                return Optional.Empty;
            }

            UpgradeSave(saveDir);

            PersistedWorldData persistedData = LoadDataFromPath(saveDir);

            if (persistedData == null)
            {
                return Optional.Empty;
            }

            World world = CreateWorld(persistedData, config.GameMode);

            return Optional.Of(world);
        }

        internal PersistedWorldData LoadDataFromPath(string saveDir)
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
                    Log.Error($"Could not load world, creating a new one : {ex.GetType()} {ex.Message}");

                    // Backup world if loading fails
                    string outZip = Path.Combine(saveDir, "worldBackup.zip");
                    Log.WarnSensitive("Creating a backup at {path}", Path.GetFullPath(outZip));
                    FileSystem.Instance.ZipFilesInDirectory(saveDir, outZip, $"*{FileEnding}", true);
                }
            }

            return null;
        }

        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile(Path.Combine(WorldManager.SavesFolderDir, config.SaveName));
            if (fileLoadedWorld.HasValue)
            {
                return fileLoadedWorld.Value;
            }

            return CreateFreshWorld();
        }

        private World CreateFreshWorld()
        {
            PersistedWorldData pWorldData = new()
            {
                EntityData = EntityData.From(new List<Entity>()),
                PlayerData = PlayerData.From(new List<Player>()),
                WorldData = new WorldData()
                {
                    GameData = new GameData
                    {
                        PDAState = new PDAStateData(),
                        StoryGoals = new StoryGoalData(),
                        StoryTiming = new StoryTimingData()
                    },
                    ParsedBatchCells = new List<NitroxInt3>(),
                    Seed = config.Seed
                },
                GlobalRootData = new()
            };

            return CreateWorld(pWorldData, config.GameMode);
        }

        public World CreateWorld(PersistedWorldData pWorldData, NitroxGameMode gameMode)
        {
            string seed = pWorldData.WorldData.Seed;
            if (string.IsNullOrWhiteSpace(seed))
            {
#if DEBUG
                seed = "TCCBIBZXAB";
#else
                seed = StringHelper.GenerateRandomString(10);
#endif
            }

            Log.Info($"Loading world with seed {seed}");

            EntityRegistry entityRegistry = NitroxServiceLocator.LocateService<EntityRegistry>();
            entityRegistry.AddEntities(pWorldData.EntityData.Entities);
            foreach (Entity entity in pWorldData.GlobalRootData.Entities)
            {
                Log.Debug($"Adding GlobalRootEntity: {entity.Id} of type: {entity.GetType()}");
            }
            entityRegistry.AddEntitiesIgnoringDuplicate(pWorldData.GlobalRootData.Entities.OfType<Entity>().ToList());

            World world = new()
            {
                SimulationOwnershipData = new SimulationOwnershipData(),
                PlayerManager = new PlayerManager(pWorldData.PlayerData.GetPlayers(), config),

                EscapePodManager = new EscapePodManager(entityRegistry, randomStart, seed),

                EntityRegistry = entityRegistry,

                GameData = pWorldData.WorldData.GameData,
                GameMode = gameMode,
                Seed = seed
            };

            world.TimeKeeper = new(world.PlayerManager, pWorldData.WorldData.GameData.StoryTiming.ElapsedSeconds);
            world.StoryManager = new(world.PlayerManager, pWorldData.WorldData.GameData.PDAState, pWorldData.WorldData.GameData.StoryGoals, world.TimeKeeper, seed, pWorldData.WorldData.GameData.StoryTiming.AuroraCountdownTime, pWorldData.WorldData.GameData.StoryTiming.AuroraWarningTime);
            world.ScheduleKeeper = new ScheduleKeeper(pWorldData.WorldData.GameData.PDAState, pWorldData.WorldData.GameData.StoryGoals, world.TimeKeeper, world.PlayerManager);

            world.BatchEntitySpawner = new BatchEntitySpawner(
                NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                NitroxServiceLocator.LocateService<UweWorldEntityFactory>(),
                NitroxServiceLocator.LocateService<UwePrefabFactory>(),
                pWorldData.WorldData.ParsedBatchCells,
                protoBufSerializer,
                NitroxServiceLocator.LocateService<Dictionary<NitroxTechType, IEntityBootstrapper>>(),
                NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>(),
                world.Seed
            );

            world.WorldEntityManager = new WorldEntityManager(world.EntityRegistry, world.BatchEntitySpawner);

            world.BuildingManager = new(world.EntityRegistry, world.WorldEntityManager, config);

            HashSet<NitroxTechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<NitroxTechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityRegistry, world.WorldEntityManager, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);

            return world;
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
                Log.Error(ex, $"Error while upgrading save file. \"Version{FileEnding}\" couldn't be read.");
                return;
            }

            if (saveFileVersion == null || saveFileVersion.Version == NitroxEnvironment.Version)
            {
                return;
            }

            if (config.SerializerMode == ServerSerializerMode.PROTOBUF)
            {
                Log.Info("Can't upgrade while using ProtoBuf as serializer");
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
                    Log.Error(ex, "Error while upgrading save file.");
                    return;
                }

                Serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
                Log.Info($"Save file was upgraded to {NitroxEnvironment.Version}");
            }
        }
    }
}
