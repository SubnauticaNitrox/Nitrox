using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using NitroxServer.GameLogic.Unlockables;
using NitroxModel.DataStructures;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        private readonly ServerProtobufSerializer serializer;
        private readonly ServerConfig config;

        public WorldPersistence(ServerProtobufSerializer serializer, ServerConfig config)
        {
            this.serializer = serializer;
            this.config = config;
        }

        public void Save(World world)
        {
            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                persistedData.WorldData.ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches;
                persistedData.WorldData.ServerStartTime = world.TimeKeeper.ServerStartTime;
                persistedData.WorldData.EntityData = EntityData.From(world.EntityManager.GetAllEntities());
                persistedData.BaseData = BaseData.From(world.BaseManager.GetPartiallyConstructedPieces(), world.BaseManager.GetCompletedBasePieceHistory());
                persistedData.WorldData.VehicleData = VehicleData.From(world.VehicleManager.GetVehicles());
                persistedData.WorldData.InventoryData = InventoryData.From(world.InventoryManager.GetAllInventoryItems(), world.InventoryManager.GetAllStorageSlotItems());
                persistedData.PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers());
                persistedData.WorldData.GameData = world.GameData;
                persistedData.WorldData.StoryTimingData = StoryTimingData.From(world.EventTriggerer);
                persistedData.WorldData.EscapePodData = EscapePodData.From(world.EscapePodManager.GetEscapePods());

                if (!Directory.Exists(config.SaveName))
                {
                    Directory.CreateDirectory(config.SaveName);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(config.SaveName, "BaseData.nitrox")))
                {
                    serializer.Serialize(stream, new SaveVersion(BaseData.VERSION));
                    serializer.Serialize(stream, persistedData.BaseData);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(config.SaveName, "PlayerData.nitrox")))
                {
                    serializer.Serialize(stream, new SaveVersion(PlayerData.VERSION));
                    serializer.Serialize(stream, persistedData.PlayerData);
                }

                using (Stream stream = File.OpenWrite(Path.Combine(config.SaveName, "WorldData.nitrox")))
                {
                    serializer.Serialize(stream, new SaveVersion(WorldData.VERSION));
                    serializer.Serialize(stream, persistedData.WorldData);
                }

                Log.Info("World state saved.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not save world: " + ex);
            }
        }

        private Optional<World> LoadFromFile()
        {
            try
            {
                if (!Directory.Exists(config.SaveName))
                {
                    throw new DirectoryNotFoundException();
                }

                PersistedWorldData persistedData = new PersistedWorldData();

                using (Stream stream = File.OpenRead(Path.Combine(config.SaveName, "BaseData.nitrox")))
                {
                    SaveVersion version = serializer.Deserialize<SaveVersion>(stream);
                    if (version.Version != BaseData.VERSION)
                    {
                        throw new VersionMismatchException("BaseData file is too old");
                    }
                    persistedData.BaseData = serializer.Deserialize<BaseData>(stream);
                }

                using (Stream stream = File.OpenRead(Path.Combine(config.SaveName, "PlayerData.nitrox")))
                {
                    SaveVersion version = serializer.Deserialize<SaveVersion>(stream);
                    if (version.Version != PlayerData.VERSION)
                    {
                        throw new VersionMismatchException("PlayerData file is too old");
                    }
                    persistedData.PlayerData = serializer.Deserialize<PlayerData>(stream);
                }

                using (Stream stream = File.OpenRead(Path.Combine(config.SaveName, "WorldData.nitrox")))
                {
                    SaveVersion version = serializer.Deserialize<SaveVersion>(stream);
                    if (version.Version != WorldData.VERSION)
                    {
                        throw new VersionMismatchException("WorldData file is too old");
                    }

                    persistedData.WorldData = serializer.Deserialize<WorldData>(stream);
                }

                if (persistedData == null || !persistedData.IsValid())
                {
                    throw new InvalidDataException("Persisted state is not valid");
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
            catch (DirectoryNotFoundException)
            {
                Log.Info("No previous save file found - creating a new one.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not load world: " + ex + " creating a new one.");
            }

            return Optional.Empty;
        }

        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile();
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
                                                              serializer,
                                                              NitroxServiceLocator.LocateService<Dictionary<TechType, IEntityBootstrapper>>(),
                                                              NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>());

            world.EntityManager = new EntityManager(entities, world.BatchEntitySpawner);

            HashSet<TechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<TechType>>();
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
