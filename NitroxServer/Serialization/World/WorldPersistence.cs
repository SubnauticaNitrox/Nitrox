﻿using System;
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
        /// <summary>
        ///  For nitrox save files
        /// </summary>
        private IServerSerializer saveDataSerializer;
        private string fileEnding;
        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerConfig config;
        private readonly RandomStartGenerator randomStart;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, ServerConfig config, RandomStartGenerator randomStart)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.config = config;
            this.randomStart = randomStart;

            saveDataSerializer = config.SerializerMode == ServerSerializerMode.PROTOBUF ? (IServerSerializer)protoBufSerializer : jsonSerializer;
            fileEnding = saveDataSerializer.GetFileEnding();
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

                saveDataSerializer.Serialize(Path.Combine(saveDir, "Version" + fileEnding), new SaveFileVersions());
                saveDataSerializer.Serialize(Path.Combine(saveDir, "BaseData" + fileEnding), persistedData.BaseData);
                saveDataSerializer.Serialize(Path.Combine(saveDir, "PlayerData" + fileEnding), persistedData.PlayerData);
                saveDataSerializer.Serialize(Path.Combine(saveDir, "WorldData" + fileEnding), persistedData.WorldData);
                saveDataSerializer.Serialize(Path.Combine(saveDir, "EntityData" + fileEnding), persistedData.EntityData);

                Log.Info("World state saved.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Could not save world: " + ex);
                return false;
            }
        }

        internal Optional<World> LoadFromFile(string saveDir)
        {
            if (!Directory.Exists(saveDir) || !File.Exists(Path.Combine(saveDir, "Version" + fileEnding)))
            {
                Log.Warn("No previous save file found - creating a new one.");
                return Optional.Empty;
            }

            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                SaveFileVersions versions = saveDataSerializer.Deserialize<SaveFileVersions>(Path.Combine(saveDir, "Version" + fileEnding));

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
                persistedData.EntityData = saveDataSerializer.Deserialize<EntityData>(Path.Combine(saveDir, "EntityData" + fileEnding));

                if (!persistedData.IsValid())
                {
                    throw new InvalidDataException("Save files are not valid");
                }


                World world = CreateWorld(persistedData,
                                          config.GameMode);

                return Optional.Of(world);
            }
            catch (Exception ex)
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
#if DEBUG
                Log.Error($"Could not load world, creating a new one: {ex}");
#else
                Log.Warn($"Could not load world, creating a new one");
#endif
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
            PersistedWorldData pWorldData = new PersistedWorldData
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
                    InventoryData = InventoryData.From(new List<ItemData>(), new List<ItemData>()),
                    VehicleData = VehicleData.From(new List<VehicleModel>()),
                    ParsedBatchCells = new List<NitroxInt3>(),
                    ServerStartTime = DateTime.Now
#if DEBUG
                , Seed = "TCCBIBZXAB"
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

            World world = new World
            {
                TimeKeeper = new TimeKeeper { ServerStartTime = pWorldData.WorldData.ServerStartTime },

                SimulationOwnershipData = new SimulationOwnershipData(),
                PlayerManager = new PlayerManager(pWorldData.PlayerData.GetPlayers(), config),

                BaseManager = new BaseManager(pWorldData.BaseData.PartiallyConstructedPieces, pWorldData.BaseData.CompletedBasePieceHistory),

                InventoryManager = new InventoryManager(pWorldData.WorldData.InventoryData.InventoryItems, pWorldData.WorldData.InventoryData.StorageSlotItems),

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
            saveDataSerializer = serializer;
            fileEnding = serializer.GetFileEnding();
        }
    }
}
