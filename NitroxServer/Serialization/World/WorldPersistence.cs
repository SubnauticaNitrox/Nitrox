﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using NitroxServer.Serialization.Resources.Datastructures;

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

                saveDataSerializer.Serialize(Path.Combine(saveDir, "Version" + fileEnding), new SaveFileVersions());
                saveDataSerializer.Serialize(Path.Combine(saveDir, "BaseData" + fileEnding), persistedData.BaseData);
                saveDataSerializer.Serialize(Path.Combine(saveDir, "PlayerData" + fileEnding), persistedData.PlayerData);
                saveDataSerializer.Serialize(Path.Combine(saveDir, "WorldData" + fileEnding), persistedData.WorldData);

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


                versions = saveDataSerializer.Deserialize<SaveFileVersions>(Path.Combine(saveDir, "Version" + fileEnding));

                if (versions == null)
                {
                    throw new InvalidDataException("Version file is empty or corrupted");
                }

                if (versions.BaseDataVersion != BaseData.VERSION)
                {
                    throw new VersionMismatchException("BaseData file is too old");
                }

                if (versions.PlayerDataVersion != PlayerData.VERSION)
                {
                    throw new VersionMismatchException("PlayerData file is too old");
                }

                if (versions.WorldDataVersion != WorldData.VERSION)
                {
                    throw new VersionMismatchException("WorldData file is too old");
                }

                persistedData.BaseData = saveDataSerializer.Deserialize<BaseData>(Path.Combine(saveDir, "BaseData" + fileEnding));
                persistedData.PlayerData = saveDataSerializer.Deserialize<PlayerData>(Path.Combine(saveDir, "PlayerData" + fileEnding));
                persistedData.WorldData = saveDataSerializer.Deserialize<WorldData>(Path.Combine(saveDir, "WorldData" + fileEnding));

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
                                          config.GameModeEnum);

                return Optional.Of(world);
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                {
                    Log.Warn("No previous save file found - creating a new one.");
                }
                else
                {
                    //Backup world if loading fails
                    using (ZipFile zipFile = new ZipFile())
                    {
                        zipFile.AddFile(Path.Combine(saveDir, "Version" + fileEnding));
                        zipFile.AddFile(Path.Combine(saveDir, "BaseData" + fileEnding));
                        zipFile.AddFile(Path.Combine(saveDir, "PlayerData" + fileEnding));
                        zipFile.AddFile(Path.Combine(saveDir, "WorldData" + fileEnding));
                        zipFile.Save(Path.Combine(saveDir, "worldBackup.zip"));
                    }

                    Log.Error("Could not load world: " + ex + " creating a new one.");
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
                new List<NitroxObject>(), new List<BasePiece>(), new List<BasePiece>(),
                new List<VehicleModel>(), new List<Player>(), new List<ItemData>(),
                new List<ItemData>(),
                new GameData() { PDAState = new PDAStateData(), StoryGoals = new StoryGoalData() },
                new List<Int3>(), new List<NitroxObject>(), new StoryTimingData(), config.GameModeEnum
                );
        }

        private World CreateWorld(DateTime serverStartTime,
                                  List<NitroxObject> entities,
                                  List<BasePiece> partiallyConstructedPieces,
                                  List<BasePiece> completedBasePieceHistory,
                                  List<VehicleModel> vehicles,
                                  List<Player> players,
                                  List<ItemData> inventoryItems,
                                  List<ItemData> storageSlotItems,
                                  GameData gameData,
                                  List<Int3> parsedBatchCells,
                                  List<NitroxObject> escapePods,
                                  StoryTimingData storyTimingData,
                                  ServerGameMode gameMode)
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
            world.EscapePodManager = new EscapePodManager(escapePods.Select(o => o.GetBehavior<EscapePodModel>()).ToList());
            world.GameMode = gameMode;

            world.BatchEntitySpawner = new BatchEntitySpawner(
                NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                NitroxServiceLocator.LocateService<UweWorldEntityFactory>(),
                NitroxServiceLocator.LocateService<UwePrefabFactory>(),
                parsedBatchCells,
                protoBufSerializer,
                NitroxServiceLocator.LocateService<Dictionary<NitroxTechType, IEntityBootstrapper>>(),
                NitroxServiceLocator.LocateService<Dictionary<string, PrefabPlaceholdersGroupAsset>>()
            );

            world.EntityManager = new EntityManager(entities.Select(n => n.GetBehavior<Entity>()).ToList(), world.BatchEntitySpawner);

            HashSet<NitroxTechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<NitroxTechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityManager, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);

            return world;
        }
    }
}
