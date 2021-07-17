﻿using System;
using System.Collections.Generic;
using System.IO;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.OS;
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
        public IServerSerializer Serializer { get; private set; }
        private string FileEnding => Serializer?.FileEnding ?? "";

        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;
        private readonly RandomStartGenerator randomStart;
        private readonly ServerConfig config;

        public WorldPersistence(ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer, ServerConfig config, RandomStartGenerator randomStart)
        {
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
            this.randomStart = randomStart;
            this.config = config;

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

        public bool Save(World world, string saveDir)
        {
            try
            {
                PersistedWorldData persistedData = PersistedWorldData.From(world);

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                Serializer.Serialize(Path.Combine(saveDir, $"Version{FileEnding}"), new SaveFileVersion());
                Serializer.Serialize(Path.Combine(saveDir, $"BaseData{FileEnding}"), persistedData.BaseData);
                Serializer.Serialize(Path.Combine(saveDir, $"PlayerData{FileEnding}"), persistedData.PlayerData);
                Serializer.Serialize(Path.Combine(saveDir, $"WorldData{FileEnding}"), persistedData.WorldData);
                Serializer.Serialize(Path.Combine(saveDir, $"EntityData{FileEnding}"), persistedData.EntityData);

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
                PersistedWorldData persistedData = new();
                SaveFileVersion saveFileVersion = Serializer.Deserialize<SaveFileVersion>(Path.Combine(saveDir, $"Version{FileEnding}"));

                if (saveFileVersion == null || saveFileVersion.Version != NitroxEnvironment.Version)
                {
                    throw new InvalidDataException("Version file is empty or save data files are too old");
                }

                persistedData.BaseData = Serializer.Deserialize<BaseData>(Path.Combine(saveDir, $"BaseData{FileEnding}"));
                persistedData.PlayerData = Serializer.Deserialize<PlayerData>(Path.Combine(saveDir, $"PlayerData{FileEnding}"));
                persistedData.WorldData = Serializer.Deserialize<WorldData>(Path.Combine(saveDir, $"WorldData{FileEnding}"));
                persistedData.EntityData = Serializer.Deserialize<EntityData>(Path.Combine(saveDir, $"EntityData{FileEnding}"));

                if (!persistedData.IsValid())
                {
                    throw new InvalidDataException("Save files are not valid");
                }

                World world = CreateWorld(persistedData, config.GameMode);

                return Optional.Of(world);
            }
            catch (Exception ex)
            {
                Log.Error($"Could not load world, creating a new one : {ex.GetType()} {ex.Message}");

                //Backup world if loading fails
                string outZip = Path.Combine(saveDir, "worldBackup.zip");
                Log.WarnSensitive("Creating a backup at {path}", Path.GetFullPath(outZip));
                FileSystem.Instance.ZipFilesInDirectory(saveDir, outZip, $"*{FileEnding}", true);
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
                    ServerStartTime = DateTime.UtcNow,
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
    }
}
