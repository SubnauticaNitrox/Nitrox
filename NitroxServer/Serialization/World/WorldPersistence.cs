using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Helper;
using NitroxServer.Resources;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        public const string BACKUP_DATE_TIME_FORMAT = "yyyy-MM-dd HH.mm.ss";

        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;
        private readonly SubnauticaServerConfig config;
        private readonly RandomStartGenerator randomStart;
        private readonly IWorldModifier worldModifier;
        private readonly SaveDataUpgrade[] upgrades;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, SubnauticaServerConfig config, RandomStartGenerator randomStart, IWorldModifier worldModifier, SaveDataUpgrade[] upgrades)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
            this.config = config;
            this.randomStart = randomStart;
            this.worldModifier = worldModifier;
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

        public void BackUp(string saveDir)
        {
            if (config.MaxBackups < 1)
            {
                Log.Info($"No backup was made (\"{nameof(config.MaxBackups)}\" is equal to 0)");
                return;
            }
            string backupDir = Path.Combine(saveDir, "Backups");
            string tempOutDir = Path.Combine(backupDir, $"Backup - {DateTime.Now.ToString(BACKUP_DATE_TIME_FORMAT)}");
            Directory.CreateDirectory(backupDir);

            try
            {
                // Prepare backup location
                Directory.CreateDirectory(tempOutDir);
                string newZipFile = $"{tempOutDir}.zip";
                if (File.Exists(newZipFile))
                {
                    File.Delete(newZipFile);
                }
                foreach (string file in Directory.GetFiles(saveDir))
                {
                    File.Copy(file, Path.Combine(tempOutDir, Path.GetFileName(file)));
                }

                FileSystem.Instance.ZipFilesInDirectory(tempOutDir, newZipFile);
                Directory.Delete(tempOutDir, true);
                Log.Info("World backed up");

                // Prune old backups
                FileInfo[] backups = Directory.EnumerateFiles(backupDir)
                                              .Select(f => new FileInfo(f))
                                              .Where(f => f is { Extension: ".zip" } info && info.Name.Contains("Backup - "))
                                              .OrderBy(f => File.GetCreationTime(f.FullName))
                                              .ToArray();
                if (backups.Length > config.MaxBackups)
                {
                    int numBackupsToDelete = backups.Length - Math.Max(1, config.MaxBackups);
                    for (int i = 0; i < numBackupsToDelete; i++)
                    {
                        backups[i].Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while backing up world");
                if (Directory.Exists(tempOutDir))
                {
                    Directory.Delete(tempOutDir, true); // Delete the outZip folder that is sometimes left
                }
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
                    // Give error saying that world could not be used, and to restore a backup
                    Log.Error($"Could not load world, please restore one of your backups to continue using this world. : {ex.GetType()} {ex.Message}");

                    throw;
                }
            }

            return null;
        }

        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), config.SaveName));
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

            World newWorld = CreateWorld(pWorldData, config.GameMode);
            worldModifier.ModifyWorld(newWorld);

            return newWorld;
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
            // Initialized only once, just like UnityEngine.Random
            XORRandom.InitSeed(seed.GetHashCode());

            Log.Info($"Loading world with seed {seed}");

            EntityRegistry entityRegistry = NitroxServiceLocator.LocateService<EntityRegistry>();
            entityRegistry.AddEntities(pWorldData.EntityData.Entities);
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

            world.TimeKeeper = new(world.PlayerManager, pWorldData.WorldData.GameData.StoryTiming.ElapsedSeconds, pWorldData.WorldData.GameData.StoryTiming.RealTimeElapsed);
            world.StoryManager = new(world.PlayerManager, pWorldData.WorldData.GameData.PDAState, pWorldData.WorldData.GameData.StoryGoals, world.TimeKeeper, seed, pWorldData.WorldData.GameData.StoryTiming.AuroraCountdownTime, pWorldData.WorldData.GameData.StoryTiming.AuroraWarningTime, pWorldData.WorldData.GameData.StoryTiming.AuroraRealExplosionTime);
            world.ScheduleKeeper = new ScheduleKeeper(pWorldData.WorldData.GameData.PDAState, pWorldData.WorldData.GameData.StoryGoals, world.TimeKeeper, world.PlayerManager);

            world.BatchEntitySpawner = new BatchEntitySpawner(
                NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                NitroxServiceLocator.LocateService<IUweWorldEntityFactory>(),
                NitroxServiceLocator.LocateService<IUwePrefabFactory>(),
                pWorldData.WorldData.ParsedBatchCells,
                protoBufSerializer,
                NitroxServiceLocator.LocateService<IEntityBootstrapperManager>(),
                NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>(),
                pWorldData.WorldData.GameData.PDAState,
                world.Seed
            );

            world.WorldEntityManager = new WorldEntityManager(world.EntityRegistry, world.BatchEntitySpawner);

            world.BuildingManager = new(world.EntityRegistry, world.WorldEntityManager, config);

            ISimulationWhitelist simulationWhitelist = NitroxServiceLocator.LocateService<ISimulationWhitelist>();
            world.EntitySimulation = new EntitySimulation(world.EntityRegistry, world.WorldEntityManager, world.SimulationOwnershipData, world.PlayerManager, simulationWhitelist);

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
