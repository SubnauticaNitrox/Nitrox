using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Server;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.GameLogic.Vehicles;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        public IServerSerializer SaveDataSerializer { get; private set; }

        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly RandomStartGenerator randomStart;
        private readonly ServerConfig config;
        private string fileEnding;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, ServerConfig config, RandomStartGenerator randomStart)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.randomStart = randomStart;
            this.config = config;

            SaveDataSerializer = config.SerializerMode == ServerSerializerMode.PROTOBUF
                                ? protoBufSerializer
                                : jsonSerializer;
            fileEnding = SaveDataSerializer.GetFileEnding();
        }

        public bool Save(World world, string saveDir)
        {
            try
            {
                PersistedWorldData persistedData = PersistedWorldData.From(world);

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                SaveDataSerializer.Serialize(Path.Combine(saveDir, $"Version{fileEnding}"), new SaveFileVersion());
                SaveDataSerializer.Serialize(Path.Combine(saveDir, $"BaseData{fileEnding}"), persistedData.BaseData);
                SaveDataSerializer.Serialize(Path.Combine(saveDir, $"PlayerData{fileEnding}"), persistedData.PlayerData);
                SaveDataSerializer.Serialize(Path.Combine(saveDir, $"WorldData{fileEnding}"), persistedData.WorldData);
                SaveDataSerializer.Serialize(Path.Combine(saveDir, $"EntityData{fileEnding}"), persistedData.EntityData);

                Log.Info("World state saved.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Could not save world: {ex}");
                return false;
            }
        }

        internal Optional<World> LoadFromFile(string saveDir)
        {
            if (!Directory.Exists(saveDir) || !File.Exists(Path.Combine(saveDir, $"Version{fileEnding}")))
            {
                Log.Warn("No previous save file found, creating a new one");
                return Optional.Empty;
            }

            try
            {
                PersistedWorldData persistedData = new();
                SaveFileVersion saveFileVersion = SaveDataSerializer.Deserialize<SaveFileVersion>(Path.Combine(saveDir, $"Version{fileEnding}"));

                if (saveFileVersion == null || saveFileVersion.Version != NitroxEnvironment.Version)
                {
                    throw new InvalidDataException("Version file is empty or save data files are too old");
                }

                persistedData.BaseData = SaveDataSerializer.Deserialize<BaseData>(Path.Combine(saveDir, $"BaseData{fileEnding}"));
                persistedData.PlayerData = SaveDataSerializer.Deserialize<PlayerData>(Path.Combine(saveDir, $"PlayerData{fileEnding}"));
                persistedData.WorldData = SaveDataSerializer.Deserialize<WorldData>(Path.Combine(saveDir, $"WorldData{fileEnding}"));
                persistedData.EntityData = SaveDataSerializer.Deserialize<EntityData>(Path.Combine(saveDir, $"EntityData{fileEnding}"));

                if (!persistedData.IsValid())
                {
                    throw new InvalidDataException("Save files are not valid");
                }

                World world = CreateWorld(persistedData, config.GameMode);

                return Optional.Of(world);
            }
            catch (Exception ex)
            {
                Log.Error($"Could not load world, creating a new one: {ex.Message}");

                //Backup world if loading fails
                using (ZipFile zipFile = new(Path.Combine(saveDir, "worldBackup.zip")))
                {
                    string[] nitroxFiles = Directory.GetFiles(saveDir, $"*{fileEnding}");
                    zipFile.AddFiles(nitroxFiles);
                    zipFile.Save();
                    Log.Warn($"Creating a backup at {zipFile.Name}");
                }

            }

            return Optional.Empty;
        }

        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile(config.SaveName);
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
                    ServerStartTime = DateTime.Now,
#if DEBUG
                    Seed = "TCCBIBZXAB"
#endif
                }
            };

            return CreateWorld(pWorldData, config.GameMode);
        }

        public World CreateWorld(PersistedWorldData pWorldData, ServerGameMode gameMode)
        {
            string seed = pWorldData.WorldData.Seed;
            if (string.IsNullOrWhiteSpace(seed))
            {
                seed = StringHelper.GenerateRandomString(10);
            }

            Log.Info($"Loading world with seed {seed}");

            World world = new()
            {
                TimeKeeper = new TimeKeeper { ServerStartTime = pWorldData.WorldData.ServerStartTime },

                SimulationOwnershipData = new SimulationOwnershipData(),
                PlayerManager = new PlayerManager(pWorldData.PlayerData.GetPlayers(), config),

                BaseManager = new BaseManager(pWorldData.BaseData.PartiallyConstructedPieces, pWorldData.BaseData.CompletedBasePieceHistory),

                InventoryManager = new InventoryManager(pWorldData.WorldData.InventoryData.InventoryItems, pWorldData.WorldData.InventoryData.StorageSlotItems, pWorldData.WorldData.InventoryData.Modules),

                EscapePodManager = new EscapePodManager(pWorldData.WorldData.EscapePodData.EscapePods, randomStart, seed),

                GameData = pWorldData.WorldData.GameData,
                GameMode = gameMode,
                Seed = seed
            };

            world.EventTriggerer = new EventTriggerer(world.PlayerManager, pWorldData.WorldData.GameData.StoryTiming.ElapsedTime, pWorldData.WorldData.GameData.StoryTiming.AuroraExplosionTime);
            world.VehicleManager = new VehicleManager(pWorldData.WorldData.VehicleData.Vehicles, world.InventoryManager);

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

            world.EntityManager = new EntityManager(pWorldData.EntityData.Entities, world.BatchEntitySpawner);

            HashSet<NitroxTechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<NitroxTechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityManager, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);

            return world;
        }

        internal void UpdateSerializer(IServerSerializer serializer)
        {
            SaveDataSerializer = serializer;
            fileEnding = serializer.GetFileEnding();
        }
    }
}
