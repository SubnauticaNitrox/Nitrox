using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;
using NitroxServer_Subnautica;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.GameLogic.Vehicles;
using NitroxServer.Serialization.World;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.Serialization;

[TestClass]
public class WorldPersistenceTest
{
    private static string tempSaveFilePath;
    private static PersistedWorldData worldData;
    public static PersistedWorldData[] WorldsDataAfter { get; private set; }
    public static IServerSerializer[] ServerSerializers { get; private set; }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar(), new TestAutoFacRegistrar());
        NitroxServiceLocator.BeginNewLifetimeScope();

        WorldPersistence worldPersistence = NitroxServiceLocator.LocateService<WorldPersistence>();
        ServerSerializers = NitroxServiceLocator.LocateService<IServerSerializer[]>();
        WorldsDataAfter = new PersistedWorldData[ServerSerializers.Length];
        tempSaveFilePath = Path.Combine(Path.GetTempPath(), "NitroxTestTempDir");

        worldData = GeneratePersistedWorldData();
        World.World world = worldPersistence.CreateWorld(worldData, ServerGameMode.CREATIVE);
        world.EventTriggerer.ResetWorld();

        for (int index = 0; index < ServerSerializers.Length; index++)
        {
            //Checking saving
            worldPersistence.UpdateSerializer(ServerSerializers[index]);
            Assert.IsTrue(worldPersistence.Save(world, tempSaveFilePath), $"Saving normal world failed while using {ServerSerializers[index]}.");
            Assert.IsFalse(worldPersistence.Save(null, tempSaveFilePath), $"Saving null world worked while using {ServerSerializers[index]}.");

            //Checking loading
            Optional<World.World> worldAfter = worldPersistence.LoadFromFile(tempSaveFilePath);
            Assert.IsTrue(worldAfter.HasValue, $"Loading saved world failed while using {ServerSerializers[index]}.");
            worldAfter.Value.EventTriggerer.ResetWorld();
            WorldsDataAfter[index] = PersistedWorldData.From(worldAfter.Value);
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void WorldDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.IsTrue(worldData.WorldData.ParsedBatchCells.SequenceEqual(worldDataAfter.WorldData.ParsedBatchCells));
        Assert.AreEqual(worldData.WorldData.Seed, worldDataAfter.WorldData.Seed);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void VehicleDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.WorldData.VehicleData.Vehicles.Count, worldDataAfter.WorldData.VehicleData.Vehicles.Count);
        for (int index = 0; index < worldData.WorldData.VehicleData.Vehicles.Count; index++)
        {
            VehicleModel vehicleModel = worldData.WorldData.VehicleData.Vehicles[index];
            VehicleModel vehicleModelAfter = worldDataAfter.WorldData.VehicleData.Vehicles[index];

            Assert.AreEqual(vehicleModel.TechType, vehicleModelAfter.TechType);
            Assert.AreEqual(vehicleModel.Id, vehicleModelAfter.Id);
            Assert.AreEqual(vehicleModel.Position, vehicleModelAfter.Position);
            Assert.AreEqual(vehicleModel.Rotation, vehicleModelAfter.Rotation);
            Assert.IsTrue(vehicleModel.InteractiveChildIdentifiers.SequenceEqual(vehicleModelAfter.InteractiveChildIdentifiers));
            Assert.AreEqual(vehicleModel.DockingBayId, vehicleModelAfter.DockingBayId);
            Assert.AreEqual(vehicleModel.Name, vehicleModelAfter.Name);
            Assert.IsTrue(vehicleModel.HSB.SequenceEqual(vehicleModelAfter.HSB));
            Assert.AreEqual(vehicleModel.Health, vehicleModelAfter.Health);
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void InventoryDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.WorldData.InventoryData.InventoryItems.Count, worldDataAfter.WorldData.InventoryData.InventoryItems.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.InventoryItems.Count; index++)
        {
            ItemData itemData = worldData.WorldData.InventoryData.InventoryItems[index];
            ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.InventoryItems[index];

            Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId);
            Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId);
            Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData));
        }

        Assert.AreEqual(worldData.WorldData.InventoryData.StorageSlotItems.Count, worldDataAfter.WorldData.InventoryData.StorageSlotItems.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.StorageSlotItems.Count; index++)
        {
            ItemData itemData = worldData.WorldData.InventoryData.StorageSlotItems[index];
            ItemData itemDataAfter = worldDataAfter.WorldData.InventoryData.StorageSlotItems[index];

            Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId);
            Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId);
            Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData));
        }

        Assert.AreEqual(worldData.WorldData.InventoryData.Modules.Count, worldDataAfter.WorldData.InventoryData.Modules.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.Modules.Count; index++)
        {
            EquippedItemData equippedItemData = worldData.WorldData.InventoryData.Modules[index];
            EquippedItemData equippedItemDataAfter = worldDataAfter.WorldData.InventoryData.Modules[index];

            Assert.AreEqual(equippedItemData.ContainerId, equippedItemDataAfter.ContainerId);
            Assert.AreEqual(equippedItemData.ItemId, equippedItemDataAfter.ItemId);
            Assert.IsTrue(equippedItemData.SerializedData.SequenceEqual(equippedItemDataAfter.SerializedData));
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void GameDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.IsTrue(worldData.WorldData.GameData.PDAState.UnlockedTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.UnlockedTechTypes));
        Assert.IsTrue(worldData.WorldData.GameData.PDAState.KnownTechTypes.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.KnownTechTypes));
        Assert.IsTrue(worldData.WorldData.GameData.PDAState.EncyclopediaEntries.SequenceEqual(worldDataAfter.WorldData.GameData.PDAState.EncyclopediaEntries));

        Assert.AreEqual(worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count, worldDataAfter.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count);
        for (int index = 0; index < worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.Count; index++)
        {
            KeyValuePair<NitroxTechType, PDAEntry> entry = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);
            KeyValuePair<NitroxTechType, PDAEntry> entryAfter = worldData.WorldData.GameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);

            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Value.Progress, entryAfter.Value.Progress);
            Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType);
            Assert.AreEqual(entry.Value.Unlocked, entryAfter.Value.Unlocked);
        }

        Assert.AreEqual(worldData.WorldData.GameData.PDAState.PdaLog.Count, worldDataAfter.WorldData.GameData.PDAState.PdaLog.Count);
        for (int index = 0; index < worldData.WorldData.GameData.PDAState.PdaLog.Count; index++)
        {
            PDALogEntry logEntry = worldData.WorldData.GameData.PDAState.PdaLog[index];
            PDALogEntry logEntryAfter = worldDataAfter.WorldData.GameData.PDAState.PdaLog[index];

            Assert.AreEqual(logEntry.Key, logEntryAfter.Key);
            Assert.AreEqual(logEntry.Timestamp, logEntryAfter.Timestamp);
        }

        Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.CompletedGoals.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.CompletedGoals));
        Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.RadioQueue.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.RadioQueue));
        Assert.IsTrue(worldData.WorldData.GameData.StoryGoals.GoalUnlocks.SequenceEqual(worldDataAfter.WorldData.GameData.StoryGoals.GoalUnlocks));

        Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.ElapsedTime, worldDataAfter.WorldData.GameData.StoryTiming.ElapsedTime);
        Assert.AreEqual(worldData.WorldData.GameData.StoryTiming.AuroraExplosionTime, worldDataAfter.WorldData.GameData.StoryTiming.AuroraExplosionTime);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void EscapePodDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.WorldData.EscapePodData.EscapePods.Count, worldDataAfter.WorldData.EscapePodData.EscapePods.Count);
        for (int index = 0; index < worldData.WorldData.EscapePodData.EscapePods.Count; index++)
        {
            EscapePodModel escapePod = worldData.WorldData.EscapePodData.EscapePods[index];
            EscapePodModel escapePodAfter = worldDataAfter.WorldData.EscapePodData.EscapePods[index];

            Assert.AreEqual(escapePod.Id, escapePodAfter.Id);
            Assert.AreEqual(escapePod.Location, escapePodAfter.Location);
            Assert.AreEqual(escapePod.FabricatorId, escapePodAfter.FabricatorId);
            Assert.AreEqual(escapePod.MedicalFabricatorId, escapePodAfter.MedicalFabricatorId);
            Assert.AreEqual(escapePod.StorageContainerId, escapePodAfter.StorageContainerId);
            Assert.AreEqual(escapePod.RadioId, escapePodAfter.RadioId);
            Assert.IsTrue(escapePod.AssignedPlayers.SequenceEqual(escapePodAfter.AssignedPlayers));
            Assert.AreEqual(escapePod.Damaged, escapePodAfter.Damaged);
            Assert.AreEqual(escapePod.RadioDamaged, escapePodAfter.RadioDamaged);
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void BaseDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        for (int index = 0; index < worldData.BaseData.CompletedBasePieceHistory.Count; index++)
        {
            BasePieceTest(worldData.BaseData.CompletedBasePieceHistory[index], worldDataAfter.BaseData.CompletedBasePieceHistory[index]);
        }

        for (int index = 0; index < worldData.BaseData.PartiallyConstructedPieces.Count; index++)
        {
            BasePieceTest(worldData.BaseData.PartiallyConstructedPieces[index], worldDataAfter.BaseData.PartiallyConstructedPieces[index]);
        }
    }

    private static void BasePieceTest(BasePiece basePiece, BasePiece basePieceAfter)
    {
        Assert.AreEqual(basePiece.Id, basePieceAfter.Id);
        Assert.AreEqual(basePiece.ItemPosition, basePieceAfter.ItemPosition);
        Assert.AreEqual(basePiece.Rotation, basePieceAfter.Rotation);
        Assert.AreEqual(basePiece.TechType, basePieceAfter.TechType);
        Assert.AreEqual(basePiece.ParentId, basePieceAfter.ParentId);
        Assert.AreEqual(basePiece.CameraPosition, basePieceAfter.CameraPosition);
        Assert.AreEqual(basePiece.CameraRotation, basePieceAfter.CameraRotation);
        Assert.AreEqual(basePiece.ConstructionAmount, basePieceAfter.ConstructionAmount);
        Assert.AreEqual(basePiece.ConstructionCompleted, basePieceAfter.ConstructionCompleted);
        Assert.AreEqual(basePiece.IsFurniture, basePieceAfter.IsFurniture);
        Assert.AreEqual(basePiece.BaseId, basePieceAfter.BaseId);
        Assert.AreEqual(basePiece.BuildIndex, basePieceAfter.BuildIndex);

        switch (basePiece.RotationMetadata.Value)
        {
            case AnchoredFaceBuilderMetadata anchoredMetadata when basePieceAfter.RotationMetadata.Value is AnchoredFaceBuilderMetadata anchoredMetadataAfter:
                Assert.AreEqual(anchoredMetadata.Cell, anchoredMetadataAfter.Cell);
                Assert.AreEqual(anchoredMetadata.Direction, anchoredMetadataAfter.Direction);
                Assert.AreEqual(anchoredMetadata.FaceType, anchoredMetadataAfter.FaceType);
                break;
            case BaseModuleBuilderMetadata baseModuleMetadata when basePieceAfter.RotationMetadata.Value is BaseModuleBuilderMetadata baseModuleMetadataAfter:
                Assert.AreEqual(baseModuleMetadata.Cell, baseModuleMetadataAfter.Cell);
                Assert.AreEqual(baseModuleMetadata.Direction, baseModuleMetadataAfter.Direction);
                break;
            case CorridorBuilderMetadata corridorMetadata when basePieceAfter.RotationMetadata.Value is CorridorBuilderMetadata corridorMetadataAfter:
                Assert.AreEqual(corridorMetadata.Rotation, corridorMetadataAfter.Rotation);
                break;
            case MapRoomBuilderMetadata mapRoomMetadata when basePieceAfter.RotationMetadata.Value is MapRoomBuilderMetadata mapRoomMetadataAfter:
                Assert.AreEqual(mapRoomMetadata.CellType, mapRoomMetadataAfter.CellType);
                Assert.AreEqual(mapRoomMetadata.Rotation, mapRoomMetadataAfter.Rotation);
                break;
            case null when basePieceAfter.RotationMetadata.Value is null:
                break;
            default:
                Assert.Fail("BasePiece.RotationMetadata is not equal");
                break;
        }

        switch (basePiece.Metadata.Value)
        {
            case SignMetadata signMetadata when basePieceAfter.Metadata.Value is SignMetadata signMetadataAfter:
                Assert.AreEqual(signMetadata.Text, signMetadataAfter.Text);
                Assert.AreEqual(signMetadata.ColorIndex, signMetadataAfter.ColorIndex);
                Assert.AreEqual(signMetadata.ScaleIndex, signMetadataAfter.ScaleIndex);
                Assert.IsTrue(signMetadata.Elements.SequenceEqual(signMetadataAfter.Elements));
                Assert.AreEqual(signMetadata.Background, signMetadataAfter.Background);
                break;
            case null when basePieceAfter.Metadata.Value is null:
                break;
            default:
                Assert.Fail("BasePiece.Metadata is not equal");
                break;
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void PlayerDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        for (int playerIndex = 0; playerIndex < worldData.PlayerData.Players.Count; playerIndex++)
        {
            PersistedPlayerData playerData = worldData.PlayerData.Players[playerIndex];
            PersistedPlayerData playerDataAfter = worldDataAfter.PlayerData.Players[playerIndex];

            Assert.AreEqual(playerData.Name, playerDataAfter.Name);

            Assert.AreEqual(playerData.EquippedItems.Count, playerDataAfter.EquippedItems.Count);
            for (int index = 0; index < playerData.EquippedItems.Count; index++)
            {
                Assert.AreEqual(playerData.EquippedItems[index].Slot, playerDataAfter.EquippedItems[index].Slot);
                Assert.AreEqual(playerData.EquippedItems[index].TechType, playerDataAfter.EquippedItems[index].TechType);
            }

            Assert.AreEqual(playerData.Modules.Count, playerDataAfter.Modules.Count);
            for (int index = 0; index < playerData.Modules.Count; index++)
            {
                Assert.AreEqual(playerData.Modules[index].Slot, playerDataAfter.Modules[index].Slot);
                Assert.AreEqual(playerData.Modules[index].TechType, playerDataAfter.Modules[index].TechType);
            }

            Assert.AreEqual(playerData.UsedItems.Count, playerDataAfter.UsedItems.Count);
            for (int index = 0; index < playerData.UsedItems.Count; index++)
            {
                Assert.AreEqual(playerData.UsedItems[index].Name, playerDataAfter.UsedItems[index].Name);
            }

            Assert.IsTrue(playerData.QuickSlotsBinding.SequenceEqual(playerDataAfter.QuickSlotsBinding));

            Assert.AreEqual(playerData.Id, playerDataAfter.Id);
            Assert.AreEqual(playerData.SpawnPosition, playerDataAfter.SpawnPosition);

            Assert.AreEqual(playerData.CurrentStats.Oxygen, playerDataAfter.CurrentStats.Oxygen);
            Assert.AreEqual(playerData.CurrentStats.MaxOxygen, playerDataAfter.CurrentStats.MaxOxygen);
            Assert.AreEqual(playerData.CurrentStats.Health, playerDataAfter.CurrentStats.Health);
            Assert.AreEqual(playerData.CurrentStats.Food, playerDataAfter.CurrentStats.Food);
            Assert.AreEqual(playerData.CurrentStats.Water, playerDataAfter.CurrentStats.Water);
            Assert.AreEqual(playerData.CurrentStats.InfectionAmount, playerDataAfter.CurrentStats.InfectionAmount);

            Assert.AreEqual(playerData.SubRootId, playerDataAfter.SubRootId);
            Assert.AreEqual(playerData.Permissions, playerDataAfter.Permissions);
            Assert.AreEqual(playerData.NitroxId, playerDataAfter.NitroxId);
            Assert.AreEqual(playerData.IsPermaDeath, playerDataAfter.IsPermaDeath);

            Assert.AreEqual(playerData.PingInstancePreferences.Count, playerDataAfter.PingInstancePreferences.Count);
            foreach (string pingKey in playerData.PingInstancePreferences.Keys)
            {
                Assert.AreEqual(playerData.PingInstancePreferences[pingKey].Color, playerDataAfter.PingInstancePreferences[pingKey].Color);
                Assert.AreEqual(playerData.PingInstancePreferences[pingKey].Visible, playerDataAfter.PingInstancePreferences[pingKey].Visible);
            }
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void EntityDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        for (int index = 0; index < worldData.EntityData.Entities.Count; index++)
        {
            WorldEntity entity = (WorldEntity)worldData.EntityData.Entities[index];
            WorldEntity entityAfter = (WorldEntity)worldDataAfter.EntityData.Entities[index];

            Assert.AreEqual(entity.Transform.LocalPosition, entityAfter.Transform.LocalPosition);
            Assert.AreEqual(entity.Transform.LocalRotation, entityAfter.Transform.LocalRotation);
            Assert.AreEqual(entity.Transform.LocalScale, entityAfter.Transform.LocalScale);
            Assert.AreEqual(entity.TechType, entityAfter.TechType);
            Assert.AreEqual(entity.Id, entityAfter.Id);
            Assert.AreEqual(entity.Level, entityAfter.Level);
            Assert.AreEqual(entity.ClassId, entityAfter.ClassId);
            Assert.AreEqual(entity.SpawnedByServer, entityAfter.SpawnedByServer);
            Assert.AreEqual(entity.WaterParkId, entityAfter.WaterParkId);
            Assert.AreEqual(entity.ExistsInGlobalRoot, entityAfter.ExistsInGlobalRoot);
            Assert.AreEqual(entity.ParentId, entityAfter.ParentId);
            Assert.AreEqual(entity.Metadata, entityAfter.Metadata);
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
            BaseData =
                new BaseData()
                {
                    CompletedBasePieceHistory =
                        new List<BasePiece>()
                        {
                            new BasePiece(new NitroxId(), NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.Zero, NitroxQuaternion.Identity, new NitroxTechType("BasePiece1"), Optional<NitroxId>.Of(new NitroxId()), false,
                                          Optional.Empty, Optional<BasePieceMetadata>.Of(new SignMetadata("ExampleText", 1, 2, new[] { true, false }, true)))
                        },
                    PartiallyConstructedPieces = new List<BasePiece>()
                    {
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                                      Optional<BuilderMetadata>.Of(new AnchoredFaceBuilderMetadata(new NitroxInt3(1, 2, 3), 1, 2, new NitroxInt3(0, 1, 2)))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                                      Optional<BuilderMetadata>.Of(new BaseModuleBuilderMetadata(new NitroxInt3(1, 2, 3), 1))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                                      Optional<BuilderMetadata>.Of(new CorridorBuilderMetadata(new NitroxVector3(1, 2, 3), 2, false, new NitroxInt3(4, 5, 6)))),
                        new BasePiece(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                                      Optional<BuilderMetadata>.Of(new MapRoomBuilderMetadata(0x20, 2)))
                    }
                },
            EntityData =
                new EntityData()
                {
                    Entities = new List<Entity>()
                    {
                        new WorldEntity(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 1, "PeeperClass", false, new NitroxId(), null, false, new NitroxId()),
                        new WorldEntity(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 1, "PeeperClass", false, new NitroxId(), null, true, new NitroxId())
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
                        UsedItems = new List<NitroxTechType> { new NitroxTechType("Knife"), new NitroxTechType("Flashlight") },
                        QuickSlotsBinding = new List<string> { "Test1", "Test2" },
                        EquippedItems = new List<EquippedItemData>
                        {
                            new EquippedItemData(new NitroxId(), new NitroxId(), new byte[] { 0x30, 0x40 }, "Slot3", new NitroxTechType("Flashlight")),
                            new EquippedItemData(new NitroxId(), new NitroxId(), new byte[] { 0x50, 0x9D }, "Slot4", new NitroxTechType("Knife"))
                        },
                        Modules = new List<EquippedItemData>() { new EquippedItemData(new NitroxId(), new NitroxId(), new byte[] { 0x35, 0xD0 }, "Module1", new NitroxTechType("Compass")) },
                        PingInstancePreferences = new() { { "eda14b58-cfe0-4a56-aa4a-47942567d897", new(0, false) }, { "Signal_Lifepod12", new(4, true) } }
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
                        PartiallyUnlockedByTechType = new ThreadSafeDictionary<NitroxTechType, PDAEntry>() { new KeyValuePair<NitroxTechType, PDAEntry>(new NitroxTechType("Moonpool"), new PDAEntry(new NitroxTechType("Moonpool"), 50f, 2)) },
                        PdaLog = { new PDALogEntry("key1", 1.1234f) },
                        UnlockedTechTypes = { new NitroxTechType("base") }
                    },
                    StoryGoals = new StoryGoalData()
                    {
                        CompletedGoals = { "Goal1", "Goal2" },
                        GoalUnlocks = { "Goal3", "Goal4" },
                        RadioQueue = { "Queue1" }
                    },
                    StoryTiming = new StoryTimingData() { AuroraExplosionTime = 10000, ElapsedTime = 10 },
                },
                InventoryData = new InventoryData()
                {
                    InventoryItems = new List<ItemData>() { new BasicItemData(new NitroxId(), new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }) },
                    StorageSlotItems = new List<ItemData>() { new BasicItemData(new NitroxId(), new NitroxId(), new byte[] { 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 }) },
                    Modules = new List<EquippedItemData>() { new EquippedItemData(new NitroxId(), new NitroxId(), new byte[] { 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 }, "Slot1", new NitroxTechType("")) }
                },
                ParsedBatchCells = new List<NitroxInt3>() { new NitroxInt3(10, 1, 10), new NitroxInt3(15, 4, 12) },
                VehicleData = new VehicleData()
                {
                    Vehicles = new List<VehicleModel>()
                    {
                        new CyclopsModel(new NitroxTechType("Cyclops"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, Array.Empty<InteractiveChildObjectIdentifier>(), Optional<NitroxId>.Of(new NitroxId()), "Super Duper Cyclops",
                                         new[] { NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One }, 100)
                    }
                },
                Seed = "NITROXSEED"
            }
        };
    }
}

public class DynamicWorldDataAfterAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        return WorldPersistenceTest.WorldsDataAfter.Select((t, i) => new object[] { t, WorldPersistenceTest.ServerSerializers[i].GetType().Name });
    }

    public string GetDisplayName(MethodInfo methodInfo, object[] data)
    {
        return data != null ? $"{methodInfo.Name} ({data[1]})" : $"{methodInfo.Name} (no-data)";
    }
}
