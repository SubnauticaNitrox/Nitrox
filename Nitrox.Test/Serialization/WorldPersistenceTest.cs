using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;
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
        private static string tempSaveFilePath;
        private static IServerSerializer[] serverSerializers;
        private static PersistedWorldData worldData;
        private static PersistedWorldData[] worldsDataAfter;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar(), new TestAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            WorldPersistence worldPersistence = NitroxServiceLocator.LocateService<WorldPersistence>();
            serverSerializers = NitroxServiceLocator.LocateService<IServerSerializer[]>();
            worldsDataAfter = new PersistedWorldData[serverSerializers.Length];
            tempSaveFilePath = Path.Combine(Path.GetTempPath(), "NitroxTestTempDir");

            worldData = GeneratePersistedWorldData();
            World world = worldPersistence.CreateWorld(worldData, ServerGameMode.CREATIVE);
            world.EventTriggerer.ResetWorld();

            for (int index = 0; index < serverSerializers.Length; index++)
            {
                //Checking saving
                worldPersistence.UpdateSerializer(serverSerializers[index]);
                Assert.IsTrue(worldPersistence.Save(world, tempSaveFilePath), $"Saving normal world failed while using {serverSerializers[index]}.");
                Assert.IsFalse(worldPersistence.Save(null, tempSaveFilePath), $"Saving null world worked while using {serverSerializers[index]}.");

                //Checking loading
                Optional<World> worldAfter = worldPersistence.LoadFromFile(tempSaveFilePath);
                Assert.IsTrue(worldAfter.HasValue, $"Loading saved world failed while using {serverSerializers[index]}.");
                worldAfter.Value.EventTriggerer.ResetWorld();
                worldsDataAfter[index] = PersistedWorldData.From(worldAfter.Value);
            }
        }


        [TestMethod]
        public void WorldDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                Assert.IsTrue(worldData.WorldData.ParsedBatchCells.SequenceEqual(worldDataAfter.WorldData.ParsedBatchCells), $"WorldData.ParsedBatchCells is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.AreEqual(worldData.WorldData.Seed, worldDataAfter.WorldData.Seed, $"WorldData.Seed is not equal while using {serverSerializers[serializerIndex]}.");
            }
        }

        [TestMethod]
        public void VehicleDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                Assert.AreEqual(worldData.WorldData.VehicleData.Vehicles.Count, worldDataAfter.WorldData.VehicleData.Vehicles.Count, $"WorldData.VehicleData.Vehicles.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.VehicleData.Vehicles.Count; index++)
                {
                    VehicleModel vehicleModel = worldData.WorldData.VehicleData.Vehicles[index];
                    VehicleModel vehicleModelAfter = worldDataAfter.WorldData.VehicleData.Vehicles[index];

                    Assert.AreEqual(vehicleModel.TechType, vehicleModelAfter.TechType, $"WorldData.VehicleData.Vehicles.TechType is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.Id, vehicleModelAfter.Id, $"WorldData.VehicleData.Vehicles.Id is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.Position, vehicleModelAfter.Position, $"WorldData.VehicleData.Vehicles.Position is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.Rotation, vehicleModelAfter.Rotation, $"WorldData.VehicleData.Vehicles.Rotation is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(vehicleModel.InteractiveChildIdentifiers.SequenceEqual(vehicleModelAfter.InteractiveChildIdentifiers), $"WorldData.VehicleData.Vehicles.InteractiveChildIdentifiers is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.DockingBayId, vehicleModelAfter.DockingBayId, $"WorldData.VehicleData.Vehicles.DockingBayId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.Name, vehicleModelAfter.Name, $"WorldData.VehicleData.Vehicles.Name is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(vehicleModel.HSB.SequenceEqual(vehicleModelAfter.HSB), $"WorldData.VehicleData.Vehicles.HSB is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(vehicleModel.Health, vehicleModelAfter.Health, $"WorldData.VehicleData.Vehicles.Health is not equal while using {serverSerializers[serializerIndex]}.");
                }
            }
        }

        [TestMethod]
        public void InventoryDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                Assert.AreEqual(worldData.WorldData.InventoryData.InventoryItems.Count, worldDataAfter.WorldData.InventoryData.InventoryItems.Count, $"WorldData.InventoryData.InventoryItems.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.InventoryData.InventoryItems.Count; index++)
                {
                    ItemData itemData = worldData.WorldData.InventoryData.InventoryItems[index];
                    ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.InventoryItems[index];

                    Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId, $"WorldData.InventoryData.InventoryItems.ContainerId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId, $"WorldData.InventoryData.InventoryItems.ItemId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData), $"WorldData.InventoryData.InventoryItems.SerializedData is not equal while using {serverSerializers[serializerIndex]}.");
                }

                Assert.AreEqual(worldData.WorldData.InventoryData.StorageSlotItems.Count, worldDataAfter.WorldData.InventoryData.StorageSlotItems.Count, $"WorldData.InventoryData.StorageSlotItems.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.InventoryData.StorageSlotItems.Count; index++)
                {
                    ItemData itemData = worldData.WorldData.InventoryData.StorageSlotItems[index];
                    ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.StorageSlotItems[index];

                    Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId, $"WorldData.InventoryData.StorageSlotItems.ContainerId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId, $"WorldData.InventoryData.StorageSlotItems.ItemId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData), $"WorldData.InventoryData.StorageSlotItems.SerializedData is not equal while using {serverSerializers[serializerIndex]}.");
                }

                Assert.AreEqual(worldData.WorldData.InventoryData.Modules.Count, worldDataAfter.WorldData.InventoryData.Modules.Count, $"WorldData.InventoryData.Modules.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.InventoryData.Modules.Count; index++)
                {
                    EquippedItemData equippedItemData = worldData.WorldData.InventoryData.Modules[index];
                    EquippedItemData equippedItemDataAfter = worldDataAfter.WorldData.InventoryData.Modules[index];

                    Assert.AreEqual(equippedItemData.ContainerId, equippedItemDataAfter.ContainerId, $"WorldData.InventoryData.Modules.ContainerId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(equippedItemData.ItemId, equippedItemDataAfter.ItemId, $"WorldData.InventoryData.Modules.ItemId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(equippedItemData.SerializedData.SequenceEqual(equippedItemDataAfter.SerializedData), $"WorldData.InventoryData.Modules.SerializedData is not equal while using {serverSerializers[serializerIndex]}.");
                }
            }
        }

        [TestMethod]
        public void GameDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.UnlockedTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.UnlockedTechTypes), $"WorldData.GameData.PDAState.UnlockedTechTypes is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.KnownTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.KnownTechTypes), $"WorldData.GameData.PDAState.KnownTechTypes is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.IsTrue(worldData.WorldData.GameData.PDAState.EncyclopediaEntries.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.EncyclopediaEntries), $"WorldData.GameData.PDAState.EncyclopediaEntries is not equal while using {serverSerializers[serializerIndex]}.");

                Assert.AreEqual(worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count, worldDataAfter.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count, $"WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count; index++)
                {
                    KeyValuePair<NitroxTechType, PDAEntry> entry = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);
                    KeyValuePair<NitroxTechType, PDAEntry> entryAfter = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);

                    Assert.AreEqual(entry.Key, entryAfter.Key, $"WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Key is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entry.Value.Progress, entryAfter.Value.Progress, $"WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.Progress is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType, $"WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.TechType is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entry.Value.Unlocked, entryAfter.Value.Unlocked, $"WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Value.Unlocked is not equal while using {serverSerializers[serializerIndex]}.");
                }

                Assert.AreEqual(worldData.WorldData.GameData.PDAState.PdaLog.Count, worldDataAfter.WorldData.GameData.PDAState.PdaLog.Count, $"WorldData.GameData.PDAState.PdaLog.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.GameData.PDAState.PdaLog.Count; index++)
                {
                    PDALogEntry logEntry = worldData.WorldData.GameData.PDAState.PdaLog[index];
                    PDALogEntry logEntryAfter = worldDataAfter.WorldData.GameData.PDAState.PdaLog[index];

                    Assert.AreEqual(logEntry.Key, logEntryAfter.Key, $"WorldData.GameData.PDAState.PdaLog.Key is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(logEntry.Timestamp, logEntryAfter.Timestamp, $"WorldData.GameData.PDAState.PdaLog.Timestamp is not equal while using {serverSerializers[serializerIndex]}.");
                }

                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.CompletedGoals.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.CompletedGoals), $"WorldData.GameData.StoryGoals.CompletedGoals is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.RadioQueue.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.RadioQueue), $"WorldData.GameData.StoryGoals.RadioQueue is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.GoalUnlocks.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.GoalUnlocks), $"WorldData.GameData.StoryGoals.GoalUnlocks is not equal while using {serverSerializers[serializerIndex]}.");

                Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.ElapsedTime, worldDataAfter.WorldData.GameData.StoryTiming.ElapsedTime, $"WorldData.GameData.StoryTiming.ElapsedTime is not equal while using {serverSerializers[serializerIndex]}.");
                Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.AuroraExplosionTime, worldDataAfter.WorldData.GameData.StoryTiming.AuroraExplosionTime, $"WorldData.GameData.StoryTiming.AuroraExplosionTime is not equal while using {serverSerializers[serializerIndex]}.");
            }
        }

        [TestMethod]
        public void EscapePodDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                Assert.AreEqual(worldData.WorldData.EscapePodData.EscapePods.Count, worldDataAfter.WorldData.EscapePodData.EscapePods.Count, $"WorldData.EscapePodData.EscapePods.Count is not equal while using {serverSerializers[serializerIndex]}.");
                for (int index = 0; index < worldData.WorldData.EscapePodData.EscapePods.Count; index++)
                {
                    EscapePodModel escapePod = worldData.WorldData.EscapePodData.EscapePods[index];
                    EscapePodModel escapePodAfter = worldDataAfter.WorldData.EscapePodData.EscapePods[index];

                    Assert.AreEqual(escapePod.Id, escapePodAfter.Id, $"WorldData.EscapePodData.EscapePods.Id is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.Location, escapePodAfter.Location, $"WorldData.EscapePodData.EscapePods.Location is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.FabricatorId, escapePodAfter.FabricatorId, $"WorldData.EscapePodData.EscapePods.FabricatorId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.MedicalFabricatorId, escapePodAfter.MedicalFabricatorId, $"WorldData.EscapePodData.EscapePods.MedicalFabricatorId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.StorageContainerId, escapePodAfter.StorageContainerId, $"WorldData.EscapePodData.EscapePods.StorageContainerId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.RadioId, escapePodAfter.RadioId, $"WorldData.EscapePodData.EscapePods.RadioId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(escapePod.AssignedPlayers.SequenceEqual(escapePodAfter.AssignedPlayers), $"WorldData.EscapePodData.EscapePods.AssignedPlayers is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.Damaged, escapePodAfter.Damaged, $"WorldData.EscapePodData.EscapePods.Damaged is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(escapePod.RadioDamaged, escapePodAfter.RadioDamaged, $"WorldData.EscapePodData.EscapePods.RadioDamaged is not equal while using {serverSerializers[serializerIndex]}.");
                }
            }
        }

        [TestMethod]
        public void BaseDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                for (int index = 0; index < worldData.BaseData.CompletedBasePieceHistory.Count; index++)
                {
                    BasePieceTest(worldData.BaseData.CompletedBasePieceHistory[index], worldsDataAfter[serializerIndex].BaseData.CompletedBasePieceHistory[index], serializerIndex);
                }

                for (int index = 0; index < worldData.BaseData.PartiallyConstructedPieces.Count; index++)
                {
                    BasePieceTest(worldData.BaseData.PartiallyConstructedPieces[index], worldsDataAfter[serializerIndex].BaseData.PartiallyConstructedPieces[index], serializerIndex);
                }
            }
        }

        private void BasePieceTest(BasePiece basePiece, BasePiece basePieceAfter, int serializerIndex)
        {
            Assert.AreEqual(basePiece.Id, basePieceAfter.Id, $"BasePiece.Id is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.ItemPosition, basePieceAfter.ItemPosition, $"BasePiece.ItemPosition is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.Rotation, basePieceAfter.Rotation, $"BasePiece.Rotation is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.TechType, basePieceAfter.TechType, $"BasePiece.TechType is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.ParentId, basePieceAfter.ParentId, $"BasePiece.ParentId is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.CameraPosition, basePieceAfter.CameraPosition, $"BasePiece.CameraPosition is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.CameraRotation, basePieceAfter.CameraRotation, $"BasePiece.CameraRotation is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.ConstructionAmount, basePieceAfter.ConstructionAmount, $"BasePiece.ConstructionAmount is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.ConstructionCompleted, basePieceAfter.ConstructionCompleted, $"BasePiece.ConstructionCompleted is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.IsFurniture, basePieceAfter.IsFurniture, $"BasePiece.IsFurniture is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.BaseId, basePieceAfter.BaseId, $"BasePiece.BaseId is not equal while using {serverSerializers[serializerIndex]}.");
            Assert.AreEqual(basePiece.BuildIndex, basePieceAfter.BuildIndex, $"BasePiece.BuildIndex is not equal while using {serverSerializers[serializerIndex]}.");

            switch (basePiece.RotationMetadata.Value)
            {
                case AnchoredFaceBuilderMetadata anchoredMetadata when basePieceAfter.RotationMetadata.Value is AnchoredFaceBuilderMetadata anchoredMetadataAfter:
                    Assert.AreEqual(anchoredMetadata.Cell, anchoredMetadataAfter.Cell, $"BasePiece.RotationMetadata.Cell (AnchoredFaceRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(anchoredMetadata.Direction, anchoredMetadataAfter.Direction, $"BasePiece.RotationMetadata.Direction (AnchoredFaceRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(anchoredMetadata.FaceType, anchoredMetadataAfter.FaceType, $"BasePiece.RotationMetadata.FaceType (AnchoredFaceRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
                case BaseModuleBuilderMetadata baseModuleMetadata when basePieceAfter.RotationMetadata.Value is BaseModuleBuilderMetadata baseModuleMetadataAfter:
                    Assert.AreEqual(baseModuleMetadata.Cell, baseModuleMetadataAfter.Cell, $"BasePiece.RotationMetadata.Cell (BaseModuleRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(baseModuleMetadata.Direction, baseModuleMetadataAfter.Direction, $"BasePiece.RotationMetadata.Direction (BaseModuleRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
                case CorridorBuilderMetadata corridorMetadata when basePieceAfter.RotationMetadata.Value is CorridorBuilderMetadata corridorMetadataAfter:
                    Assert.AreEqual(corridorMetadata.Rotation, corridorMetadataAfter.Rotation, $"BasePiece.RotationMetadata.Rotation (CorridorRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
                case MapRoomBuilderMetadata mapRoomMetadata when basePieceAfter.RotationMetadata.Value is MapRoomBuilderMetadata mapRoomMetadataAfter:
                    Assert.AreEqual(mapRoomMetadata.CellType, mapRoomMetadataAfter.CellType, $"BasePiece.RotationMetadata.CellType (MapRoomRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(mapRoomMetadata.ConnectionMask, mapRoomMetadataAfter.ConnectionMask, $"BasePiece.RotationMetadata.ConnectionMask (MapRoomRotationMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
                case null when basePieceAfter.RotationMetadata.Value is null:
                    break;
                default:
                    Assert.Fail($"BasePiece.RotationMetadata is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
            }

            switch (basePiece.Metadata.Value)
            {
                case SignMetadata signMetadata when basePieceAfter.Metadata.Value is SignMetadata signMetadataAfter:
                    Assert.AreEqual(signMetadata.Text, signMetadataAfter.Text, $"BasePiece.Metadata.Text (SignMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(signMetadata.ColorIndex, signMetadataAfter.ColorIndex, $"BasePiece.Metadata.ColorIndex (SignMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(signMetadata.ScaleIndex, signMetadataAfter.ScaleIndex, $"BasePiece.Metadata.ScaleIndex (SignMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(signMetadata.Elements.SequenceEqual(signMetadataAfter.Elements), $"BasePiece.Metadata.Elements (SignMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(signMetadata.Background, signMetadataAfter.Background, $"BasePiece.Metadata.Background (SignMetadata) is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
                case null when basePieceAfter.Metadata.Value is null:
                    break;
                default:
                    Assert.Fail($"BasePiece.Metadata is not equal while using {serverSerializers[serializerIndex]}.");
                    break;
            }
        }

        [TestMethod]
        public void PlayerDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                for (int playerIndex = 0; playerIndex < worldData.PlayerData.Players.Count; playerIndex++)
                {
                    PersistedPlayerData playerData = worldData.PlayerData.Players[playerIndex];
                    PersistedPlayerData playerDataAfter = worldDataAfter.PlayerData.Players[playerIndex];

                    Assert.AreEqual(playerData.Name, playerDataAfter.Name, $"PlayerData.Players.Name is not equal while using {serverSerializers[serializerIndex]}.");

                    Assert.AreEqual(playerData.EquippedItems.Count, playerDataAfter.EquippedItems.Count, $"PlayerData.Players.EquippedItems.Count is not equal while using {serverSerializers[serializerIndex]}.");
                    for (int index = 0; index < playerData.EquippedItems.Count; index++)
                    {
                        Assert.AreEqual(playerData.EquippedItems[index].Slot, playerDataAfter.EquippedItems[index].Slot, $"PlayerData.Players.EquippedItems.Slot is not equal while using {serverSerializers[serializerIndex]}.");
                        Assert.AreEqual(playerData.EquippedItems[index].TechType, playerDataAfter.EquippedItems[index].TechType, $"PlayerData.Players.EquippedItems.TechType is not equal while using {serverSerializers[serializerIndex]}.");
                    }

                    Assert.AreEqual(playerData.Modules.Count, playerDataAfter.Modules.Count, $"PlayerData.Players.Modules.Count is not equal while using {serverSerializers[serializerIndex]}.");
                    for (int index = 0; index < playerData.Modules.Count; index++)
                    {
                        Assert.AreEqual(playerData.Modules[index].Slot, playerDataAfter.Modules[index].Slot, $"PlayerData.Players.Modules.Slot is not equal while using {serverSerializers[serializerIndex]}.");
                        Assert.AreEqual(playerData.Modules[index].TechType, playerDataAfter.Modules[index].TechType, $"PlayerData.Players.Modules.TechType is not equal while using {serverSerializers[serializerIndex]}.");
                    }

                    Assert.AreEqual(playerData.UsedItems.Count, playerDataAfter.UsedItems.Count, $"PlayerData.Players.UsedItems.Count is not equal while using {serverSerializers[serializerIndex]}.");
                    for (int index = 0; index < playerData.UsedItems.Count; index++)
                    {
                        Assert.AreEqual(playerData.UsedItems[index].Name, playerDataAfter.UsedItems[index].Name, $"PlayerData.Players.UsedItems.Name is not equal while using {serverSerializers[serializerIndex]}.");
                    }

                    Assert.IsTrue(playerData.QuickSlotsBinding.SequenceEqual(playerDataAfter.QuickSlotsBinding), $"PlayerData.Players.QuickSlotsBinding is not equal while using {serverSerializers[serializerIndex]}.");

                    Assert.AreEqual(playerData.Id, playerDataAfter.Id, $"PlayerData.Players.Id is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.SpawnPosition, playerDataAfter.SpawnPosition, $"PlayerData.Players.SpawnPosition is not equal while using {serverSerializers[serializerIndex]}.");

                    Assert.AreEqual(playerData.CurrentStats.Oxygen, playerDataAfter.CurrentStats.Oxygen, $"PlayerData.Players.CurrentStats.Oxygen is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.CurrentStats.MaxOxygen, playerDataAfter.CurrentStats.MaxOxygen, $"PlayerData.Players.CurrentStats.MaxOxygen is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.CurrentStats.Health, playerDataAfter.CurrentStats.Health, $"PlayerData.Players.CurrentStats.Health is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.CurrentStats.Food, playerDataAfter.CurrentStats.Food, $"PlayerData.Players.CurrentStats.Food is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.CurrentStats.Water, playerDataAfter.CurrentStats.Water, $"PlayerData.Players.CurrentStats.Water is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.CurrentStats.InfectionAmount, playerDataAfter.CurrentStats.InfectionAmount, $"PlayerData.Players.CurrentStats.InfectionAmount is not equal while using {serverSerializers[serializerIndex]}.");

                    Assert.AreEqual(playerData.SubRootId, playerDataAfter.SubRootId, $"PlayerData.Players.SubRootId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.Permissions, playerDataAfter.Permissions, $"PlayerData.Players.Permissions is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.NitroxId, playerDataAfter.NitroxId, $"PlayerData.Players.NitroxId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(playerData.IsPermaDeath, playerDataAfter.IsPermaDeath, $"PlayerData.Players.IsFurniture is not equal while using {serverSerializers[serializerIndex]}.");

                    foreach (string pingKey in playerData.PingInstancePreferences.Keys)
                    {
                        Assert.AreEqual(playerDataAfter.PingInstancePreferences[pingKey], playerDataAfter.PingInstancePreferences[pingKey], $"PlayerData.Players.PingInstancePreferences is not equal while using {serverSerializers[serializerIndex]}.");
                    }
                    Assert.AreEqual(playerData.PingInstancePreferences.Count, playerDataAfter.PingInstancePreferences.Count, $"PlayerData.Players.PingInstancePreferences is not equal while using {serverSerializers[serializerIndex]}.");
                }
            }
        }

        [TestMethod]
        public void EntityDataTest()
        {
            for (int serializerIndex = 0; serializerIndex < worldsDataAfter.Length; serializerIndex++)
            {
                PersistedWorldData worldDataAfter = worldsDataAfter[serializerIndex];
                for (int index = 0; index < worldData.EntityData.Entities.Count; index++)
                {
                    Entity entity = worldData.EntityData.Entities[index];
                    Entity entityAfter = worldDataAfter.EntityData.Entities[index];

                    Assert.AreEqual(entity.Transform.LocalPosition, entityAfter.Transform.LocalPosition, $"EntityData.Entities.Transform.LocalPosition is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.Transform.LocalRotation, entityAfter.Transform.LocalRotation, $"EntityData.Entities.Transform.LocalRotation is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.Transform.LocalScale, entityAfter.Transform.LocalScale, $"EntityData.Entities.Transform.LocalPosition is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.TechType, entityAfter.TechType, $"EntityData.Entities.TechType is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.Id, entityAfter.Id, $"EntityData.Entities.Id is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.Level, entityAfter.Level, $"EntityData.Entities.Level is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.ClassId, entityAfter.ClassId, $"EntityData.Entities.ClassId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.SpawnedByServer, entityAfter.SpawnedByServer, $"EntityData.Entities.SpawnedByServer is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.WaterParkId, entityAfter.WaterParkId, $"EntityData.Entities.WaterParkId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.IsTrue(entity.SerializedGameObject.SequenceEqual(entityAfter.SerializedGameObject), $"EntityData.Entities.SerializedGameObject is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.ExistsInGlobalRoot, entityAfter.ExistsInGlobalRoot, $"EntityData.Entities.ExistsInGlobalRoot is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.ParentId, entityAfter.ParentId, $"EntityData.Entities.ParentId is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.Metadata, entityAfter.Metadata, $"EntityData.Entities.Metadata is not equal while using {serverSerializers[serializerIndex]}.");
                    Assert.AreEqual(entity.ExistingGameObjectChildIndex, entityAfter.ExistingGameObjectChildIndex, "EntityData.Entities.ExistingGameObjectChildIndex is not equal while using {serverSerializers[serializerIndex]}.");
                }
            }
        }


        [ClassCleanup]
        public static void ClassCleanup()
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
                        new BasePiece(new NitroxId(), NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.Zero, NitroxQuaternion.Identity, new NitroxTechType("BasePiece1"), Optional<NitroxId>.Of(new NitroxId()), false, Optional.Empty, Optional<BasePieceMetadata>.Of(new SignMetadata("ExampleText", 1, 2, new [] {true,false}, true)))
                    },
                    PartiallyConstructedPieces = new List<BasePiece>()
                    {
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<BuilderMetadata>.Of(new AnchoredFaceBuilderMetadata(new NitroxInt3(1,2,3), 1, 2))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<BuilderMetadata>.Of(new BaseModuleBuilderMetadata(new NitroxInt3(1,2,3), 1))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<BuilderMetadata>.Of(new CorridorBuilderMetadata(new NitroxVector3(1,2,3),2, false, default))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false, Optional<BuilderMetadata>.Of(new MapRoomBuilderMetadata(0x20, 2)))
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
                            UsedItems = new List<NitroxTechType>(0),
                            QuickSlotsBinding = new List<string>(0),
                            EquippedItems = new List<EquippedItemData>(0),
                            Modules = new List<EquippedItemData>(0),
                            PingInstancePreferences = new()
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
                            UsedItems = new List<NitroxTechType> {new NitroxTechType("Knife"), new NitroxTechType("Flashlight")},
                            QuickSlotsBinding = new List<string>{"Test1", "Test2"},
                            EquippedItems = new List<EquippedItemData>
                            {
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x30, 0x40}, "Slot3", new NitroxTechType("Flashlight")),
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x50, 0x9D}, "Slot4", new NitroxTechType("Knife"))
                            },
                            Modules = new List<EquippedItemData>()
                            {
                                new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{0x35, 0xD0}, "Module1", new NitroxTechType("Compass"))
                            },
                            PingInstancePreferences = new(){
                                { "eda14b58-cfe0-4a56-aa4a-47942567d897", new(0, false) },
                                { "Signal_Lifepod12", new(4, true) }
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
                        },
                        Modules = new List<EquippedItemData>()
                        {
                            new EquippedItemData(new NitroxId(), new NitroxId(), new byte[]{ 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 }, "Slot1", new NitroxTechType(""))
                        }
                    },
                    ParsedBatchCells = new List<NitroxInt3>()
                    {
                        new NitroxInt3(10,1,10),
                        new NitroxInt3(15,4,12)
                    },
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
