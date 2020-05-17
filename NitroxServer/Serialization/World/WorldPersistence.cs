using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
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
using System;
using System.Collections.Generic;
using System.IO;
using NitroxServer.GameLogic.Unlockables;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Server;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        /// <summary>
        ///  For nitrox save files
        /// </summary>
        private readonly IServerSerializer saveDataSerializer;
        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly string fileEnding;
        private readonly ServerConfig config;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, ServerConfig config)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.config = config;

            Log.Debug($"Using {config.SerializerModeEnum} as save file serializer");
            saveDataSerializer = config.SerializerModeEnum == ServerSerializerMode.PROTOBUF ? (IServerSerializer)protoBufSerializer : jsonSerializer;
            fileEnding = config.SerializerModeEnum == ServerSerializerMode.PROTOBUF ? ".nitrox" : ".json";
        }

        public void Save(World world, string saveDir)
        {
            try
            {
                PersistedWorldData persistedData = new PersistedWorldData
                {
                    BaseData = BaseData.From(world.BaseManager.GetPartiallyConstructedPieces(), world.BaseManager.GetCompletedBasePieceHistory()),
                    PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers()),
                    WorldData =
                    {
                        ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches,
                        ServerStartTime = world.TimeKeeper.ServerStartTime,
                        EntityData = EntityData.From(world.EntityManager.GetAllEntities()),
                        VehicleData = VehicleData.From(world.VehicleManager.GetVehicles()),
                        InventoryData = InventoryData.From(world.InventoryManager.GetAllInventoryItems(), world.InventoryManager.GetAllStorageSlotItems()),
                        GameData = world.GameData,
                        StoryTimingData = StoryTimingData.From(world.EventTriggerer),
                        EscapePodData = EscapePodData.From(world.EscapePodManager.GetEscapePods())
                    }
                };

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(saveDir, "Version" + fileEnding)))
                {
                    saveDataSerializer.Serialize(stream, new SaveFileVersions());
                }

                using (Stream stream = File.OpenWrite(Path.Combine(saveDir, "BaseData" + fileEnding)))
                {
                    saveDataSerializer.Serialize(stream, persistedData.BaseData);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(saveDir, "PlayerData" + fileEnding)))
                {
                    saveDataSerializer.Serialize(stream, persistedData.PlayerData);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(saveDir, "WorldData" + fileEnding)))
                {
                    saveDataSerializer.Serialize(stream, persistedData.WorldData);
                }

                Log.Info("World state saved.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not save world: " + ex);
            }
        }

        private Optional<World> LoadFromFile(string saveDir)
        {
            try
            {
                if (!Directory.Exists(saveDir))
                {
                    throw new DirectoryNotFoundException();
                }

                PersistedWorldData persistedData = new PersistedWorldData();
                SaveFileVersions versions;

                using (Stream stream = File.OpenRead(Path.Combine(saveDir, "Version" + fileEnding)))
                {
                    versions = saveDataSerializer.Deserialize<SaveFileVersions>(stream);
                    if (versions == null)
                    {
                        throw new InvalidDataException("Version file is empty or corrupted");
                    }
                }

                using (Stream stream = File.OpenRead(Path.Combine(saveDir, "BaseData" + fileEnding)))
                {
                    if (versions.BaseDataVersion != BaseData.VERSION)
                    {
                        throw new VersionMismatchException("BaseData file is too old");
                    }
                    persistedData.BaseData = saveDataSerializer.Deserialize<BaseData>(stream);
                }

                using (Stream stream = File.OpenRead(Path.Combine(saveDir, "PlayerData" + fileEnding)))
                {
                    if (versions.PlayerDataVersion != PlayerData.VERSION)
                    {
                        throw new VersionMismatchException("PlayerData file is too old");
                    }
                    persistedData.PlayerData = saveDataSerializer.Deserialize<PlayerData>(stream);
                }

                using (Stream stream = File.OpenRead(Path.Combine(saveDir, "WorldData" + fileEnding)))
                {
                    if (versions.WorldDataVersion != WorldData.VERSION)
                    {
                        throw new VersionMismatchException("WorldData file is too old");
                    }

                    persistedData.WorldData = saveDataSerializer.Deserialize<WorldData>(stream);
                }

                if (!persistedData.IsValid())
                {
                    throw new InvalidDataException("Save files are not valid");
                }


                World world = CreateWorld(persistedData.WorldData.ServerStartTime.Value,
                                          persistedData.WorldData.EntityData.Entities,
                                          persistedData.BaseData.PartiallyConstructedPieces,
                                          persistedData.BaseData.CompletedBasePieceHistory,
                                          persistedData.WorldData.VehicleData.Vehicles,
                                          persistedData.PlayerData.GetPlayers(),
                                          persistedData.WorldData.InventoryData.InventoryItems,
                                          persistedData.WorldData.InventoryData.StorageSlotItems,
                                          persistedData.WorldData.GameData,
                                          persistedData.WorldData.ParsedBatchCells,
                                          persistedData.WorldData.EscapePodData.EscapePods,
                                          persistedData.WorldData.StoryTimingData,
                                          config.GameMode);

                return Optional.Of(world);
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                {
                    Log.Info("No previous save file found - creating a new one.");
                }
                else
                {
                    Log.Info("Could not load world: " + ex + " creating a new one.");
                }

                //Backup world if loading fails
                using (ZipFile zipFile = new ZipFile(Path.Combine(config.SaveName, "worldBackup.zip")))
                {
                    zipFile.AddFile(Path.Combine(config.SaveName, "Version" + fileEnding));
                    zipFile.AddFile(Path.Combine(config.SaveName, "BaseData" + fileEnding));
                    zipFile.AddFile(Path.Combine(config.SaveName, "PlayerData" + fileEnding));
                    zipFile.AddFile(Path.Combine(config.SaveName, "WorldData" + fileEnding));
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
            return CreateWorld(
                DateTime.Now,
                new List<Entity>(), new List<BasePiece>(), new List<BasePiece>(),
                new List<VehicleModel>(), new List<Player>(), new List<ItemData>(),
                new List<ItemData>(),
                new GameData() { PDAState = new PDAStateData(), StoryGoals = new StoryGoalData() },
                new List<Int3>(), new List<EscapePodModel>(), new StoryTimingData(), config.GameMode);
        }

        private World CreateWorld(DateTime serverStartTime,
                                  List<Entity> entities,
                                  List<BasePiece> partiallyConstructedPieces,
                                  List<BasePiece> completedBasePieceHistory,
                                  List<VehicleModel> vehicles,
                                  List<Player> players,
                                  List<ItemData> inventoryItems,
                                  List<ItemData> storageSlotItems,
                                  GameData gameData,
                                  List<Int3> parsedBatchCells,
                                  List<EscapePodModel> escapePods,
                                  StoryTimingData storyTimingData,
                                  string gameMode)
        {
            World world = new World();
            world.TimeKeeper = new TimeKeeper();
            world.TimeKeeper.ServerStartTime = serverStartTime;

            world.SimulationOwnershipData = new SimulationOwnershipData();
            world.PlayerManager = new PlayerManager(players, config);
            world.EventTriggerer = new EventTriggerer(world.PlayerManager, storyTimingData.ElapsedTime, storyTimingData.AuroraExplosionTime);
            world.BaseManager = new BaseManager(partiallyConstructedPieces, completedBasePieceHistory);
            world.InventoryManager = new InventoryManager(inventoryItems, storageSlotItems);
            world.VehicleManager = new VehicleManager(vehicles, world.InventoryManager);
            world.GameData = gameData;
            world.EscapePodManager = new EscapePodManager(escapePods);
            world.GameMode = gameMode;

            world.BatchEntitySpawner = new BatchEntitySpawner(NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                                                              NitroxServiceLocator.LocateService<UweWorldEntityFactory>(),
                                                              NitroxServiceLocator.LocateService<UwePrefabFactory>(),
                                                              parsedBatchCells,
                                                              protoBufSerializer,
                                                              NitroxServiceLocator.LocateService<Dictionary<NitroxTechType, IEntityBootstrapper>>(),
                                                              NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>());

            world.EntityManager = new EntityManager(entities, world.BatchEntitySpawner);

            HashSet<NitroxTechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<NitroxTechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityManager, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);

            Log.Info($"World GameMode: {gameMode}");
            Log.Info($"Server Password: {(string.IsNullOrEmpty(config.ServerPassword) ? "None. Public Server." : config.ServerPassword)}");
            Log.Info($"Admin Password: {config.AdminPassword}");
            Log.Info($"Autosave: {(config.DisableAutoSave ? "DISABLED" : $"ENABLED ({config.SaveInterval / 60000} min)")}");

            Log.Info("To get help for commands, run help in console or /help in chatbox");

            return world;
        }
    }
}
