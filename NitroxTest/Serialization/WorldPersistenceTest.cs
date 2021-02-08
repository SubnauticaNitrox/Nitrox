using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.GameLogic.Vehicles;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;
using NitroxServer_Subnautica;

namespace NitroxTest.Serialization
{
    [TestClass]
    public class WorldPersistenceTest
    {
        private string tempSaveFilePath;
        private WorldPersistence worldPersistence;
        private World world;

        private ServerConfig serverConfig;
        private IServerSerializer[] serverSerializers;

        private PersistedWorldData worldData;
        private PersistedWorldData[] worldsDataAfter;

        [TestInitialize]
        public void Setup()
        {
            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar(), new TestAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            tempSaveFilePath = Path.Combine(Path.GetTempPath(), "NitroxTestTempDir");
            serverSerializers = NitroxServiceLocator.LocateService<IServerSerializer[]>();
            worldPersistence = NitroxServiceLocator.LocateService<WorldPersistence>();
            serverConfig = NitroxServiceLocator.LocateService<ServerConfig>();

            worldData = GeneratePersistedWorldData();
            world = worldPersistence.CreateWorld(worldData, serverConfig.GameMode);
            world.EventTriggerer.PauseEventTimers();
            world.EventTriggerer.PauseWorldTime();

            worldsDataAfter = new PersistedWorldData[serverSerializers.Length];
            for (int index = 0; index < serverSerializers.Length; index++)
            {
                worldPersistence.UpdateSerializer(serverSerializers[index]);
                Assert.IsTrue(worldPersistence.Save(world, tempSaveFilePath));
                Assert.IsFalse(worldPersistence.Save(null, tempSaveFilePath));

                worldPersistence.UpdateSerializer(serverSerializers[index]);
                Optional<World> worldAfter = worldPersistence.LoadFromFile(tempSaveFilePath);
                Assert.IsTrue(worldAfter.HasValue);
                worldsDataAfter[index] = PersistedWorldData.From(worldAfter.Value);
            }
        }


        [TestMethod]
        public void WorldDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                Assert.IsTrue(worldData.WorldData.ParsedBatchCells.SequenceEqual(worldDataAfter.WorldData.ParsedBatchCells), "WorldData.ParsedBatchCells is not equal");
                Assert.AreEqual(worldData.WorldData.ServerStartTime, worldDataAfter.WorldData.ServerStartTime, "WorldData.ServerStartTime is not equal");
                Assert.AreEqual(worldData.WorldData.Seed, worldDataAfter.WorldData.Seed, "WorldData.Seed is not equal");
            }
        }

        [TestMethod]
        public void VehicleDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                Assert.AreEqual(worldData.WorldData.VehicleData.Vehicles.Count, worldDataAfter.WorldData.VehicleData.Vehicles.Count, "WorldData.VehicleData.Vehicles.Count is not equal");
                for (int index = 0; index < worldData.WorldData.VehicleData.Vehicles.Count; index++)
                {
                    VehicleModel vehicleModel = worldData.WorldData.VehicleData.Vehicles[index];
                    VehicleModel vehicleModelAfter = worldDataAfter.WorldData.VehicleData.Vehicles[index];

                    Assert.AreEqual(vehicleModel.TechType, vehicleModelAfter.TechType, "WorldData.VehicleData.Vehicles.TechType is not equal");
                    Assert.AreEqual(vehicleModel.Id, vehicleModelAfter.Id, "WorldData.VehicleData.Vehicles.Id is not equal");
                    Assert.AreEqual(vehicleModel.Position, vehicleModelAfter.Position, "WorldData.VehicleData.Vehicles.Position is not equal");
                    Assert.AreEqual(vehicleModel.Rotation, vehicleModelAfter.Rotation, "WorldData.VehicleData.Vehicles.Rotation is not equal");
                    Assert.IsTrue(vehicleModel.InteractiveChildIdentifiers.SequenceEqual(vehicleModelAfter.InteractiveChildIdentifiers), "WorldData.VehicleData.Vehicles.InteractiveChildIdentifiers is not equal");
                    Assert.AreEqual(vehicleModel.DockingBayId, vehicleModelAfter.DockingBayId, "WorldData.VehicleData.Vehicles.DockingBayId is not equal");
                    Assert.AreEqual(vehicleModel.Name, vehicleModelAfter.Name, "WorldData.VehicleData.Vehicles.Name is not equal");
                    Assert.IsTrue(vehicleModel.HSB.SequenceEqual(vehicleModelAfter.HSB), "WorldData.VehicleData.Vehicles.HSB is not equal");
                    Assert.AreEqual(vehicleModel.Health, vehicleModelAfter.Health, "WorldData.VehicleData.Vehicles.Health is not equal");
                }
            }
        }

        [TestMethod]
        public void InventoryDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                Assert.AreEqual(worldData.WorldData.InventoryData.InventoryItems.Count, worldDataAfter.WorldData.InventoryData.InventoryItems.Count, "WorldData.InventoryData.InventoryItems.Count is not equal");
                for (int index = 0; index < worldData.WorldData.InventoryData.InventoryItems.Count; index++)
                {
                    ItemData itemData = worldData.WorldData.InventoryData.InventoryItems[index];
                    ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.InventoryItems[index];

                    Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId, "WorldData.InventoryData.InventoryItems.ContainerId is not equal");
                    Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId, "WorldData.InventoryData.InventoryItems.ItemId is not equal");
                    Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData), "WorldData.InventoryData.InventoryItems.SerializedData is not equal");
                }

                Assert.AreEqual(worldData.WorldData.InventoryData.StorageSlotItems.Count, worldDataAfter.WorldData.InventoryData.StorageSlotItems.Count, "WorldData.InventoryData.StorageSlotItems.Count is not equal");
                for (int index = 0; index < worldData.WorldData.InventoryData.StorageSlotItems.Count; index++)
                {
                    ItemData itemData = worldData.WorldData.InventoryData.StorageSlotItems[index];
                    ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.StorageSlotItems[index];

                    Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId, "WorldData.InventoryData.StorageSlotItems.ContainerId is not equal");
                    Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId, "WorldData.InventoryData.StorageSlotItems.ItemId is not equal");
                    Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData), "WorldData.InventoryData.StorageSlotItems.SerializedData is not equal");
                }
            }
        }

        [TestMethod]
        public void GameDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.UnlockedTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.UnlockedTechTypes), "WorldData.GameData.PDAState.UnlockedTechTypes is not equal");
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.KnownTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.KnownTechTypes), "WorldData.GameData.PDAState.KnownTechTypes is not equal");
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.EncyclopediaEntries.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.EncyclopediaEntries), "WorldData.GameData.PDAState.EncyclopediaEntries is not equal");

                Assert.AreEqual(worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count, worldDataAfter.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count, "WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count is not equal");
                for (int index = 0; index < worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count; index++)
                {
                    KeyValuePair<NitroxTechType, PDAEntry> entry = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);
                    KeyValuePair<NitroxTechType, PDAEntry> entryAfter = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);

                    Assert.AreEqual(entry.Key, entryAfter.Key, "WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Key is not equal");
                    Assert.AreEqual(entry.Value.Progress, entryAfter.Value.Progress, "WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.Progress is not equal");
                    Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType, "WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.TechType is not equal");
                    Assert.AreEqual(entry.Value.Unlocked, entryAfter.Value.Unlocked, "WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.Unlocked is not equal");
                }

                Assert.AreEqual(worldData.WorldData.GameData.PDAState.PdaLog.Count, worldDataAfter.WorldData.GameData.PDAState.PdaLog.Count, "WorldData.GameData.PDAState.PdaLog.Count is not equal");
                for (int index = 0; index < worldData.WorldData.GameData.PDAState.PdaLog.Count; index++)
                {
                    PDALogEntry logEntry = worldData.WorldData.GameData.PDAState.PdaLog[index];
                    PDALogEntry logEntryAfter = worldDataAfter.WorldData.GameData.PDAState.PdaLog[index];

                    Assert.AreEqual(logEntry.Key, logEntryAfter.Key, "WorldData.GameData.PDAState.PdaLog.Key is not equal");
                    Assert.AreEqual(logEntry.Timestamp, logEntryAfter.Timestamp, "WorldData.GameData.PDAState.PdaLog.Timestamp is not equal");
                }

                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.CompletedGoals.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.CompletedGoals), "WorldData.GameData.StoryGoals.CompletedGoals is not equal");
                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.RadioQueue.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.RadioQueue), "WorldData.GameData.StoryGoals.RadioQueue is not equal");
                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.GoalUnlocks.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.GoalUnlocks), "WorldData.GameData.StoryGoals.GoalUnlocks is not equal");

                Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.ElapsedTime, worldDataAfter.WorldData.GameData.StoryTiming.ElapsedTime, "WorldData.GameData.StoryTiming.ElapsedTime is not equal");
                Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.AuroraExplosionTime, worldDataAfter.WorldData.GameData.StoryTiming.AuroraExplosionTime, "WorldData.GameData.StoryTiming.AuroraExplosionTime is not equal");
            }
        }

        [TestMethod]
        public void EscapePodDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                Assert.AreEqual(worldData.WorldData.EscapePodData.EscapePods.Count, worldDataAfter.WorldData.EscapePodData.EscapePods.Count, "WorldData.EscapePodData.EscapePods.Count is not equal");
                for (int index = 0; index < worldData.WorldData.EscapePodData.EscapePods.Count; index++)
                {
                    EscapePodModel escapePod = worldData.WorldData.EscapePodData.EscapePods[index];
                    EscapePodModel escapePodAfter = worldDataAfter.WorldData.EscapePodData.EscapePods[index];

                    Assert.AreEqual(escapePod.Id, escapePodAfter.Id, "WorldData.EscapePodData.EscapePods.Id is not equal");
                    Assert.AreEqual(escapePod.Location, escapePodAfter.Location, "WorldData.EscapePodData.EscapePods.Location is not equal");
                    Assert.AreEqual(escapePod.FabricatorId, escapePodAfter.FabricatorId, "WorldData.EscapePodData.EscapePods.FabricatorId is not equal");
                    Assert.AreEqual(escapePod.MedicalFabricatorId, escapePodAfter.MedicalFabricatorId, "WorldData.EscapePodData.EscapePods.MedicalFabricatorId is not equal");
                    Assert.AreEqual(escapePod.StorageContainerId, escapePodAfter.StorageContainerId, "WorldData.EscapePodData.EscapePods.StorageContainerId is not equal");
                    Assert.AreEqual(escapePod.RadioId, escapePodAfter.RadioId, "WorldData.EscapePodData.EscapePods.RadioId is not equal");
                    Assert.IsTrue(escapePod.AssignedPlayers.SequenceEqual(escapePodAfter.AssignedPlayers), "WorldData.EscapePodData.EscapePods.AssignedPlayers is not equal");
                    Assert.AreEqual(escapePod.Damaged, escapePodAfter.Damaged, "WorldData.EscapePodData.EscapePods.Damaged is not equal");
                    Assert.AreEqual(escapePod.RadioDamaged, escapePodAfter.RadioDamaged, "WorldData.EscapePodData.EscapePods.RadioDamaged is not equal");
                }
            }
        }

        [TestMethod]
        public void BaseDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                for (int index = 0; index < worldData.BaseData.CompletedBasePieceHistory.Count; index++)
                {
                    BasePiece basePiece = worldData.BaseData.CompletedBasePieceHistory[index];
                    BasePiece basePieceAfter = worldDataAfter.BaseData.CompletedBasePieceHistory[index];

                    Assert.AreEqual(basePiece.Id, basePieceAfter.Id, "BaseData.CompletedBasePieceHistory.Id is not equal");
                    Assert.AreEqual(basePiece.ItemPosition, basePieceAfter.ItemPosition, "BaseData.CompletedBasePieceHistory.ItemPosition is not equal");
                    Assert.AreEqual(basePiece.Rotation, basePieceAfter.Rotation, "BaseData.CompletedBasePieceHistory.Rotation is not equal");
                    Assert.AreEqual(basePiece.TechType, basePieceAfter.TechType, "BaseData.CompletedBasePieceHistory.TechType is not equal");
                    Assert.AreEqual(basePiece.ParentId, basePieceAfter.ParentId, "BaseData.CompletedBasePieceHistory.ParentId is not equal");
                    Assert.AreEqual(basePiece.CameraPosition, basePieceAfter.CameraPosition, "BaseData.CompletedBasePieceHistory.CameraPosition is not equal");
                    Assert.AreEqual(basePiece.CameraRotation, basePieceAfter.CameraRotation, "BaseData.CompletedBasePieceHistory.CameraRotation is not equal");
                    Assert.AreEqual(basePiece.ConstructionAmount, basePieceAfter.ConstructionAmount, "BaseData.CompletedBasePieceHistory.ConstructionAmount is not equal");
                    Assert.AreEqual(basePiece.ConstructionCompleted, basePieceAfter.ConstructionCompleted, "BaseData.CompletedBasePieceHistory.ConstructionCompleted is not equal");
                    Assert.AreEqual(basePiece.IsFurniture, basePieceAfter.IsFurniture, "BaseData.CompletedBasePieceHistory.IsFurniture is not equal");
                    Assert.AreEqual(basePiece.BaseId, basePieceAfter.BaseId, "BaseData.CompletedBasePieceHistory.BaseId is not equal");
                    Assert.AreEqual(basePiece.BuildIndex, basePieceAfter.BuildIndex, "BaseData.CompletedBasePieceHistory.BuildIndex is not equal");

                    switch (basePiece.RotationMetadata.Value)
                    {
                        case AnchoredFaceRotationMetadata anchoredMetadata when basePieceAfter.RotationMetadata.Value is AnchoredFaceRotationMetadata anchoredMetadataAfter:
                            Assert.AreEqual(anchoredMetadata.Cell, anchoredMetadataAfter.Cell, "BaseData.RotationMetadata.Cell (AnchoredFaceRotationMetadata) is not equal");
                            Assert.AreEqual(anchoredMetadata.Direction, anchoredMetadataAfter.Direction, "BaseData.RotationMetadata.Direction (AnchoredFaceRotationMetadata) is not equal");
                            Assert.AreEqual(anchoredMetadata.FaceType, anchoredMetadataAfter.FaceType, "BaseData.RotationMetadata.FaceType (AnchoredFaceRotationMetadata) is not equal");
                            break;
                        case BaseModuleRotationMetadata baseModuleMetadata when basePieceAfter.RotationMetadata.Value is BaseModuleRotationMetadata baseModuleMetadataAfter:
                            Assert.AreEqual(baseModuleMetadata.Cell, baseModuleMetadataAfter.Cell, "BaseData.RotationMetadata.Cell (BaseModuleRotationMetadata) is not equal");
                            Assert.AreEqual(baseModuleMetadata.Direction, baseModuleMetadataAfter.Direction, "BaseData.RotationMetadata.Direction (BaseModuleRotationMetadata) is not equal");
                            break;
                        case CorridorRotationMetadata corridorMetadata when basePieceAfter.RotationMetadata.Value is CorridorRotationMetadata corridorMetadataAfter:
                            Assert.AreEqual(corridorMetadata.Rotation, corridorMetadataAfter.Rotation, "BaseData.RotationMetadata.Rotation (CorridorRotationMetadata) is not equal");
                            break;
                        case MapRoomRotationMetadata mapRoomMetadata when basePieceAfter.RotationMetadata.Value is MapRoomRotationMetadata mapRoomMetadataAfter:
                            Assert.AreEqual(mapRoomMetadata.CellType, mapRoomMetadataAfter.CellType, "BaseData.RotationMetadata.CellType (MapRoomRotationMetadata) is not equal");
                            Assert.AreEqual(mapRoomMetadata.ConnectionMask, mapRoomMetadataAfter.ConnectionMask, "BaseData.RotationMetadata.ConnectionMask (MapRoomRotationMetadata) is not equal");
                            break;
                        case null when basePieceAfter.RotationMetadata.Value is null:
                            break;
                        default:
                            Assert.Fail("BaseData.RotationMetadata is not equal");
                            break;
                    }

                    switch (basePiece.Metadata.Value)
                    {
                        case SignMetadata signMetadata when basePieceAfter.Metadata.Value is SignMetadata signMetadataAfter:
                            Assert.AreEqual(signMetadata.Text, signMetadataAfter.Text, "BaseData.Metadata.Text (SignMetadata) is not equal");
                            Assert.AreEqual(signMetadata.ColorIndex, signMetadataAfter.ColorIndex, "BaseData.Metadata.ColorIndex (SignMetadata) is not equal");
                            Assert.AreEqual(signMetadata.ScaleIndex, signMetadataAfter.ScaleIndex, "BaseData.Metadata.ScaleIndex (SignMetadata) is not equal");
                            Assert.IsTrue(signMetadata.Elements.SequenceEqual(signMetadataAfter.Elements), "BaseData.Metadata.Elements (SignMetadata) is not equal");
                            Assert.AreEqual(signMetadata.Background, signMetadataAfter.Background, "BaseData.Metadata.Background (SignMetadata) is not equal");
                            break;
                        case null when basePieceAfter.Metadata.Value is null:
                            break;
                        default:
                            Assert.Fail("BaseData.Metadata is not equal");
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void PlayerDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                for (int index = 0; index < worldData.PlayerData.Players.Count; index++)
                {
                    PersistedPlayerData playerData = worldData.PlayerData.Players[index];
                    PersistedPlayerData playerDataAfter = worldDataAfter.PlayerData.Players[index];

                    Assert.AreEqual(playerData.Name, playerDataAfter.Name, "PlayerData.Players.Name is not equal");

                    Assert.AreEqual(playerData.EquippedItems.Count, playerDataAfter.EquippedItems.Count, "PlayerData.Players.EquippedItems.Count is not equal");
                    for (int index2 = 0; index2 < playerData.EquippedItems.Count; index2++)
                    {
                        Assert.AreEqual(playerData.EquippedItems[index2].Slot, playerDataAfter.EquippedItems[index2].Slot, "PlayerData.Players.EquippedItems.Slot is not equal");
                        Assert.AreEqual(playerData.EquippedItems[index2].TechType, playerDataAfter.EquippedItems[index2].TechType, "PlayerData.Players.EquippedItems.TechType is not equal");
                    }

                    Assert.AreEqual(playerData.Modules.Count, playerDataAfter.Modules.Count, "PlayerData.Players.Modules.Count is not equal");
                    for (int index2 = 0; index2 < playerData.Modules.Count; index2++)
                    {
                        Assert.AreEqual(playerData.Modules[index2].Slot, playerDataAfter.Modules[index2].Slot, "PlayerData.Players.Modules.Slot is not equal");
                        Assert.AreEqual(playerData.Modules[index2].TechType, playerDataAfter.Modules[index2].TechType, "PlayerData.Players.Modules.TechType is not equal");
                    }

                    Assert.AreEqual(playerData.Id, playerDataAfter.Id, "PlayerData.Players.Id is not equal");
                    Assert.AreEqual(playerData.SpawnPosition, playerDataAfter.SpawnPosition, "PlayerData.Players.SpawnPosition is not equal");

                    Assert.AreEqual(playerData.CurrentStats.Oxygen, playerDataAfter.CurrentStats.Oxygen, "PlayerData.Players.CurrentStats.Oxygen is not equal");
                    Assert.AreEqual(playerData.CurrentStats.MaxOxygen, playerDataAfter.CurrentStats.MaxOxygen, "PlayerData.Players.CurrentStats.MaxOxygen is not equal");
                    Assert.AreEqual(playerData.CurrentStats.Health, playerDataAfter.CurrentStats.Health, "PlayerData.Players.CurrentStats.Health is not equal");
                    Assert.AreEqual(playerData.CurrentStats.Food, playerDataAfter.CurrentStats.Food, "PlayerData.Players.CurrentStats.Food is not equal");
                    Assert.AreEqual(playerData.CurrentStats.Water, playerDataAfter.CurrentStats.Water, "PlayerData.Players.CurrentStats.Water is not equal");
                    Assert.AreEqual(playerData.CurrentStats.InfectionAmount, playerDataAfter.CurrentStats.InfectionAmount, "PlayerData.Players.CurrentStats.InfectionAmount is not equal");

                    Assert.AreEqual(playerData.SubRootId, playerDataAfter.SubRootId, "PlayerData.Players.SubRootId is not equal");
                    Assert.AreEqual(playerData.Permissions, playerDataAfter.Permissions, "PlayerData.Players.Permissions is not equal");
                    Assert.AreEqual(playerData.NitroxId, playerDataAfter.NitroxId, "PlayerData.Players.NitroxId is not equal");
                    Assert.AreEqual(playerData.IsPermaDeath, playerDataAfter.IsPermaDeath, "PlayerData.Players.IsFurniture is not equal");
                }
            }
        }

        [TestMethod]
        public void EntityDataTest()
        {
            foreach (PersistedWorldData worldDataAfter in worldsDataAfter)
            {
                for (int index = 0; index < worldData.EntityData.Entities.Count; index++)
                {
                    Entity entity = worldData.EntityData.Entities[index];
                    Entity entityAfter = worldDataAfter.EntityData.Entities[index];

                    Assert.AreEqual(entity.Transform.LocalPosition, entityAfter.Transform.LocalPosition, "EntityData.Entities.Transform.LocalPosition is not equal");
                    Assert.AreEqual(entity.Transform.LocalRotation, entityAfter.Transform.LocalRotation, "EntityData.Entities.Transform.LocalRotation is not equal");
                    Assert.AreEqual(entity.Transform.LocalScale, entityAfter.Transform.LocalScale, "EntityData.Entities.Transform.LocalPosition is not equal");
                    Assert.AreEqual(entity.TechType, entityAfter.TechType, "EntityData.Entities.TechType is not equal");
                    Assert.AreEqual(entity.Id, entityAfter.Id, "EntityData.Entities.Id is not equal");
                    Assert.AreEqual(entity.Level, entityAfter.Level, "EntityData.Entities.Level is not equal");
                    Assert.AreEqual(entity.ClassId, entityAfter.ClassId, "EntityData.Entities.ClassId is not equal");
                    Assert.AreEqual(entity.SpawnedByServer, entityAfter.SpawnedByServer, "EntityData.Entities.SpawnedByServer is not equal");
                    Assert.AreEqual(entity.WaterParkId, entityAfter.WaterParkId, "EntityData.Entities.WaterParkId is not equal");
                    Assert.IsTrue(entity.SerializedGameObject.SequenceEqual(entityAfter.SerializedGameObject), "EntityData.Entities.SerializedGameObject is not equal");
                    Assert.AreEqual(entity.ExistsInGlobalRoot, entityAfter.ExistsInGlobalRoot, "EntityData.Entities.ExistsInGlobalRoot is not equal");
                    Assert.AreEqual(entity.ParentId, entityAfter.ParentId, "EntityData.Entities.ParentId is not equal");
                    Assert.AreEqual(entity.Metadata, entityAfter.Metadata, "EntityData.Entities.Metadata is not equal");
                    Assert.AreEqual(entity.ExistingGameObjectChildIndex, entityAfter.ExistingGameObjectChildIndex, "EntityData.Entities.ExistingGameObjectChildIndex is not equal");
                }
            }
        }


        [TestCleanup]
        public void Cleanup()
        {
            NitroxServiceLocator.EndCurrentLifetimeScope();
            if (Directory.Exists(tempSaveFilePath))
            {
                Directory.Delete(tempSaveFilePath, true);
            }
        }

        private static PersistedWorldData GeneratePersistedWorldData()
        {
            return new PersistedWorldData()
            {
                BaseData = new BaseData()
                {
                    CompletedBasePieceHistory = new List<BasePiece>()
                    {
                        new BasePiece(new NitroxId(), NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.Zero, NitroxQuaternion.Identity, new NitroxTechType("BasePiece1"), Optional<NitroxId>.Of(new NitroxId()), false, Optional.Empty)
                    },
                    PartiallyConstructedPieces = new List<BasePiece>()
                    {
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<RotationMetadata>.Of(new AnchoredFaceRotationMetadata(new NitroxInt3(1,2,3), 1, 2))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<RotationMetadata>.Of(new BaseModuleRotationMetadata(new NitroxInt3(1,2,3), 1))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<RotationMetadata>.Of(new CorridorRotationMetadata(2))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<RotationMetadata>.Of(new MapRoomRotationMetadata(0x20, 2)))
                    }
                },
                EntityData = new EntityData()
                {
                    Entities = new List<Entity>()
                    {
                        new Entity(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 1, "PeeperClass", false, new NitroxId(), new byte[]{ 0x10, 0x14, 0x0, 0x2, 0x2, 0x2, 0x2 }, false, new NitroxId()),
                        new Entity(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 1, "PeeperClass", false, new NitroxId(), new byte[]{ 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId())
                    }
                },
                PlayerData = new PlayerData()
                {
                    Players = new List<PersistedPlayerData>()
                    {
                        new PersistedPlayerData()
                        {
                            NitroxId = new NitroxId(),
                            Id = 1,
                            Name = "Test1",
                            IsPermaDeath = false,
                            Permissions = Perms.ADMIN,
                            SpawnPosition = NitroxVector3.Zero,
                            SubRootId = null,
                            CurrentStats = new PlayerStatsData(45, 45, 40, 39, 28, 1),
                            EquippedItems = new List<EquippedItemData>(0),
                            Modules = new List<EquippedItemData>(0)
                        },
                        new PersistedPlayerData()
                        {
                            NitroxId = new NitroxId(),
                            Id = 2,
                            Name = "Test2",
                            IsPermaDeath = true,
                            Permissions = Perms.PLAYER,
                            SpawnPosition = NitroxVector3.One,
                            SubRootId = new NitroxId(),
                            CurrentStats = new PlayerStatsData(40, 40, 30, 29, 28, 0),
                            EquippedItems = new List<EquippedItemData>
                            {
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x30, 0x40}, "Slot3", new NitroxTechType("Flashlight")),
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x50, 0x9D}, "Slot4", new NitroxTechType("Knife"))
                            },
                            Modules = new List<EquippedItemData>()
                            {
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x35, 0xD0}, "Module1", new NitroxTechType("Compass"))
                            }
                        }
                    }
                },
                WorldData = new WorldData()
                {
                    EscapePodData = new EscapePodData()
                    {
                        EscapePods = new List<EscapePodModel>()
                        {
                            new EscapePodModel()
                            {
                                AssignedPlayers = new List<ushort> { 1, 2 },
                                Damaged = true,
                                RadioDamaged = true,
                                Location = NitroxVector3.Zero,
                                Id = new NitroxId(),
                                FabricatorId = new NitroxId(),
                                MedicalFabricatorId = new NitroxId(),
                                RadioId = new NitroxId(),
                                StorageContainerId = new NitroxId()
                            }
                        }
                    },
                    GameData = new GameData()
                    {
                        PDAState = new PDAStateData()
                        {
                            EncyclopediaEntries = { "TestEntry1", "TestEntry2" },
                            KnownTechTypes = { new NitroxTechType("Knife") },
                            PartiallyUnlockedByTechType = new ThreadSafeDictionary<NitroxTechType, PDAEntry>()
                            {
                                new KeyValuePair<NitroxTechType, PDAEntry>(new NitroxTechType("Moonpool"), new PDAEntry(new NitroxTechType("Moonpool"), 50f, 2))
                            },
                            PdaLog = { new PDALogEntry("key1", 1.1234f) },
                            UnlockedTechTypes = { new NitroxTechType("base") }
                        },
                        StoryGoals = new StoryGoalData()
                        {
                            CompletedGoals = { "Goal1", "Goal2" },
                            GoalUnlocks = { "Goal3", "Goal4" },
                            RadioQueue = { "Queue1" }
                        },
                        StoryTiming = new StoryTimingData()
                        {
                            AuroraExplosionTime = 10000,
                            ElapsedTime = 10
                        },
                    },
                    InventoryData = new InventoryData()
                    {
                        InventoryItems = new List<ItemData>()
                        {
                            new ItemData(new NitroxId(), new NitroxId(), new byte[]{ 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 })
                        },
                        StorageSlotItems = new List<ItemData>()
                        {
                            new ItemData(new NitroxId(), new NitroxId(), new byte[]{ 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 })
                        }
                    },
                    ParsedBatchCells = new List<NitroxInt3>()
                    {
                        new NitroxInt3(10,1,10),
                        new NitroxInt3(15,4,12)
                    },
                    ServerStartTime = DateTime.UtcNow,
                    VehicleData = new VehicleData()
                    {
                        Vehicles = new List<VehicleModel>()
                        {
                            new VehicleModel(new NitroxTechType("Cyclops"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, new InteractiveChildObjectIdentifier[0], Optional<NitroxId>.Of(new NitroxId()), "Super Duper Cyclops", new []{NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One}, 100)
                        }
                    },
                    Seed = "NITROXSEED"
                }
            };
        }
    }
}
