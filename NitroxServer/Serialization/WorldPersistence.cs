using System;
using System.Collections.Generic;
using System.IO;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Server;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.WorldTiming;
using NitroxServer.Helper;
using NitroxServer.Serialization.Resources;
using NitroxServer.Serialization.Resources.DataStructures;
using NitroxServer.Serialization.SaveData;
using NitroxServer.Serialization.SaveDataUpgrades;

namespace NitroxServer.Serialization
{
    public class WorldPersistence
    {
        internal IServerSerializer serializer { get; set; }
        private string FileEnding => serializer?.FileEnding ?? "";
        private readonly ProtoBufCellParser protoBufCellParser;
        
        private readonly ServerConfig config;
        private readonly RandomStartGenerator randomStart;
        private readonly SaveDataUpgrade[] upgrades;

        public WorldPersistence(IServerSerializer serializer, ProtoBufCellParser protoBufCellParser, ServerConfig config, RandomStartGenerator randomStart, SaveDataUpgrade[] upgrades)
        {
            this.serializer = serializer;
            this.protoBufCellParser = protoBufCellParser;
            this.config = config;
            this.randomStart = randomStart;
            this.upgrades = upgrades;
        }

        public bool Save(World world, string saveDir)
        {
            try
            {
                PersistedSaveData persistedData = PersistedSaveData.From(world);

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
                serializer.Serialize(Path.Combine(saveDir, $"BaseData{FileEnding}"), persistedData.BaseData);
                serializer.Serialize(Path.Combine(saveDir, $"PlayerData{FileEnding}"), persistedData.PlayerData);
                serializer.Serialize(Path.Combine(saveDir, $"WorldData{FileEnding}"), persistedData.WorldData);
                serializer.Serialize(Path.Combine(saveDir, $"EntityData{FileEnding}"), persistedData.EntityData);

                config.Update(saveDir, c => c.Seed = persistedData.WorldData.Seed);

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

            try
            {
                PersistedSaveData persistedData = new();


                UpgradeSave(saveDir);

                persistedData.BaseData = serializer.Deserialize<BaseData>(Path.Combine(saveDir, $"BaseData{FileEnding}"));
                persistedData.PlayerData = serializer.Deserialize<PlayerData>(Path.Combine(saveDir, $"PlayerData{FileEnding}"));
                persistedData.WorldData = serializer.Deserialize<WorldData>(Path.Combine(saveDir, $"WorldData{FileEnding}"));
                persistedData.EntityData = serializer.Deserialize<EntityData>(Path.Combine(saveDir, $"EntityData{FileEnding}"));

                if (!persistedData.IsValid())
                {
                    throw new InvalidDataException("Save files are not valid");
                }

                World world = CreateWorld(persistedData, config.GameMode);

                return Optional.Of(world);
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

            return Optional.Empty;
        }
        
        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile(Path.Combine(NitroxUser.SavesFolderDir, config.SaveName));
            if (fileLoadedWorld.HasValue)
            {
                return fileLoadedWorld.Value;
            }

            return CreateFreshWorld();
        }

        private World CreateFreshWorld()
        {
            PersistedSaveData persistedData = new()
            {
                BaseData = BaseData.From(new List<BasePiece>(), new List<BasePiece>()),
                EntityData = EntityData.From(new List<Entity>()),
                PlayerData = PlayerData.From(new List<Player>()),
                WorldData = new WorldData()
                {
                    EscapePodData = EscapePodData.From(new List<EscapePodModel>()),
                    GameData = new GameData
                    {
                        PDAState = new PDAStateData(),
                        StoryGoals = new StoryGoalData(),
                        StoryTiming = new StoryTimingData()
                    },
                    InventoryData = InventoryData.From(new List<ItemData>(), new List<ItemData>(), new List<EquippedItemData>()),
                    VehicleData = VehicleData.From(new List<VehicleModel>()),
                    ParsedBatchCells = new List<NitroxInt3>(),
                    Seed = config.Seed
                }
            };

            return CreateWorld(persistedData, config.GameMode);
        }

        public World CreateWorld(PersistedSaveData persistedData, ServerGameMode gameMode)
        {
            string seed = persistedData.WorldData.Seed;
            if (string.IsNullOrWhiteSpace(seed))
            {
#if DEBUG
                seed = "TCCBIBZXAB";
#else
                seed = StringHelper.GenerateRandomString(10);
#endif
            }

            Log.Info($"Loading world with seed {seed}");

            World world = new()
            {
                SimulationOwnershipData = new SimulationOwnershipData(),
                PlayerManager = new PlayerManager(persistedData.PlayerData.GetPlayers(), config),

                BaseManager = new BaseManager(persistedData.BaseData.PartiallyConstructedPieces, persistedData.BaseData.CompletedBasePieceHistory),

                InventoryManager = new InventoryManager(persistedData.WorldData.InventoryData.InventoryItems, persistedData.WorldData.InventoryData.StorageSlotItems, persistedData.WorldData.InventoryData.Modules),

                EscapePodManager = new EscapePodManager(persistedData.WorldData.EscapePodData.EscapePods, randomStart, seed),

                GameData = persistedData.WorldData.GameData,
                GameMode = gameMode,
                Seed = seed
            };

            world.EventTriggerer = new EventTriggerer(world.PlayerManager, persistedData.WorldData.GameData.PDAState, persistedData.WorldData.GameData.StoryGoals, seed, persistedData.WorldData.GameData.StoryTiming.ElapsedTime, persistedData.WorldData.GameData.StoryTiming.AuroraExplosionTime, persistedData.WorldData.GameData.StoryTiming.AuroraWarningTime);
            world.VehicleManager = new VehicleManager(persistedData.WorldData.VehicleData.Vehicles, world.InventoryManager);
            world.ScheduleKeeper = new ScheduleKeeper(persistedData.WorldData.GameData.PDAState, persistedData.WorldData.GameData.StoryGoals, world.EventTriggerer, world.PlayerManager);

            world.BatchEntitySpawner = new BatchEntitySpawner(
                NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                NitroxServiceLocator.LocateService<UweWorldEntityFactory>(),
                NitroxServiceLocator.LocateService<UwePrefabFactory>(),
                persistedData.WorldData.ParsedBatchCells,
                protoBufCellParser,
                NitroxServiceLocator.LocateService<Dictionary<NitroxTechType, IEntityBootstrapper>>(),
                NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>(),
                world.Seed
            );

            world.EntityManager = new EntityManager(persistedData.EntityData.Entities, world.BatchEntitySpawner);

            HashSet<NitroxTechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<NitroxTechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityManager, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);

            return world;
        }

        private void UpgradeSave(string saveDir)
        {
            SaveFileVersion saveFileVersion = serializer.Deserialize<SaveFileVersion>(Path.Combine(saveDir, $"Version{FileEnding}"));

            if (saveFileVersion.Version == NitroxEnvironment.Version)
            {
                return;
            }
            
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

            serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
            Log.Info($"Save file was upgraded to {NitroxEnvironment.Version}");
            }
    }
}
