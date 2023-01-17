using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test;
using Nitrox.Test.Helper;
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
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

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
        AssertHelper.IsListEqual(worldData.WorldData.VehicleData.Vehicles.OrderBy(x => x.Id), worldDataAfter.WorldData.VehicleData.Vehicles.OrderBy(x => x.Id), (vehicleModel, vehicleModelAfter) =>
        {
            Assert.AreEqual(vehicleModel.TechType, vehicleModelAfter.TechType);
            Assert.AreEqual(vehicleModel.Id, vehicleModelAfter.Id);
            Assert.AreEqual(vehicleModel.Position, vehicleModelAfter.Position);
            Assert.AreEqual(vehicleModel.Rotation, vehicleModelAfter.Rotation);

            AssertHelper.IsListEqual(vehicleModel.InteractiveChildIdentifiers.OrderBy(x => x.Id), vehicleModelAfter.InteractiveChildIdentifiers.OrderBy(x => x.Id), (childIdentifier, childIdentifierAfter) =>
            {
                Assert.AreEqual(childIdentifier.Id, childIdentifierAfter.Id);
                Assert.AreEqual(childIdentifier.GameObjectNamePath, childIdentifierAfter.GameObjectNamePath);
            });

            Assert.AreEqual(vehicleModel.DockingBayId.HasValue, vehicleModelAfter.DockingBayId.HasValue);
            Assert.AreEqual(vehicleModel.DockingBayId.Value, vehicleModelAfter.DockingBayId.Value);
            Assert.AreEqual(vehicleModel.Name, vehicleModelAfter.Name);
            Assert.IsTrue(vehicleModel.HSB.SequenceEqual(vehicleModelAfter.HSB));
            Assert.AreEqual(vehicleModel.Health, vehicleModelAfter.Health);
        });
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void InventoryDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        AssertHelper.IsListEqual(worldData.WorldData.InventoryData.StorageSlotItems.OrderBy(x => x.ItemId), worldDataAfter.WorldData.InventoryData.StorageSlotItems.OrderBy(x => x.ItemId), ItemDataTest);
        AssertHelper.IsListEqual(worldData.WorldData.InventoryData.Modules.OrderBy(x => x.ItemId), worldDataAfter.WorldData.InventoryData.Modules.OrderBy(x => x.ItemId), ItemDataTest);
    }

    private static void ItemDataTest(ItemData itemData, ItemData itemDataAfter)
    {
        Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId);
        Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId);
        Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData));

        switch (itemData)
        {
            case BasicItemData _ when itemDataAfter is BasicItemData _:
                break;
            case EquippedItemData equippedItemData when itemDataAfter is EquippedItemData equippedItemDataAfter:
                Assert.AreEqual(equippedItemData.Slot, equippedItemDataAfter.Slot);
                Assert.AreEqual(equippedItemData.TechType, equippedItemDataAfter.TechType);
                break;
            case PlantableItemData plantableItemData when itemDataAfter is PlantableItemData plantableItemDataAfter:
                Assert.AreEqual(plantableItemData.PlantedGameTime, plantableItemDataAfter.PlantedGameTime);
                break;
            default:
                Assert.Fail($"Runtime types of {nameof(ItemData)} where not equal");
                break;
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void GameDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        PDAStateTest(worldData.WorldData.GameData.PDAState, worldDataAfter.WorldData.GameData.PDAState);
        StoryGoalTest(worldData.WorldData.GameData.StoryGoals, worldDataAfter.WorldData.GameData.StoryGoals);
        StoryTimingTest(worldData.WorldData.GameData.StoryTiming, worldDataAfter.WorldData.GameData.StoryTiming);
    }

    private static void PDAStateTest(PDAStateData pdaState, PDAStateData pdaStateAfter)
    {
        Assert.IsTrue(pdaState.UnlockedTechTypes.SequenceEqual(pdaStateAfter.UnlockedTechTypes));

        AssertHelper.IsDictionaryEqual(pdaState.PartiallyUnlockedByTechType, pdaStateAfter.PartiallyUnlockedByTechType, (entry, entryAfter) =>
        {
            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType);
            Assert.AreEqual(entry.Value.Progress, entryAfter.Value.Progress);
            Assert.AreEqual(entry.Value.Unlocked, entryAfter.Value.Unlocked);
        });

        Assert.IsTrue(pdaState.KnownTechTypes.SequenceEqual(pdaStateAfter.KnownTechTypes));
        Assert.IsTrue(pdaState.AnalyzedTechTypes.SequenceEqual(pdaStateAfter.AnalyzedTechTypes));
        Assert.IsTrue(pdaState.EncyclopediaEntries.SequenceEqual(pdaStateAfter.EncyclopediaEntries));

        AssertHelper.IsListEqual(pdaState.PdaLog.OrderBy(x => x.Key), pdaStateAfter.PdaLog.OrderBy(x => x.Key), (entry, entryAfter) =>
        {
            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Timestamp, entryAfter.Timestamp);
        });

        AssertHelper.IsDictionaryEqual(pdaState.CachedProgress, pdaStateAfter.CachedProgress, (entry, entryAfter) =>
        {
            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType);
            AssertHelper.IsDictionaryEqual(entry.Value.Entries, entryAfter.Value.Entries);
        });
    }

    private static void StoryGoalTest(StoryGoalData storyGoal, StoryGoalData storyGoalAfter)
    {
        Assert.IsTrue(storyGoal.CompletedGoals.SequenceEqual(storyGoalAfter.CompletedGoals));
        Assert.IsTrue(storyGoal.RadioQueue.SequenceEqual(storyGoalAfter.RadioQueue));
        Assert.IsTrue(storyGoal.GoalUnlocks.SequenceEqual(storyGoalAfter.GoalUnlocks));
        Assert.IsTrue(storyGoal.ScheduledGoals.SequenceEqual(storyGoalAfter.ScheduledGoals));
    }

    private static void StoryTimingTest(StoryTimingData storyTiming, StoryTimingData storyTimingAfter)
    {
        Assert.AreEqual(storyTiming.ElapsedTime, storyTimingAfter.ElapsedTime);
        Assert.AreEqual(storyTiming.AuroraExplosionTime, storyTimingAfter.AuroraExplosionTime);
        Assert.AreEqual(storyTiming.AuroraWarningTime, storyTimingAfter.AuroraWarningTime);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void BaseDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        AssertHelper.IsListEqual(worldData.BaseData.PartiallyConstructedPieces.OrderBy(x => x.Id), worldDataAfter.BaseData.PartiallyConstructedPieces.OrderBy(x => x.Id), BasePieceTest);
        AssertHelper.IsListEqual(worldData.BaseData.CompletedBasePieceHistory.OrderBy(x => x.Id), worldDataAfter.BaseData.CompletedBasePieceHistory.OrderBy(x => x.Id), BasePieceTest);
    }

    private static void BasePieceTest(BasePiece basePiece, BasePiece basePieceAfter)
    {
        Assert.AreEqual(basePiece.Id, basePieceAfter.Id);
        Assert.AreEqual(basePiece.ItemPosition, basePieceAfter.ItemPosition);
        Assert.AreEqual(basePiece.Rotation, basePieceAfter.Rotation);
        Assert.AreEqual(basePiece.TechType, basePieceAfter.TechType);
        Assert.AreEqual(basePiece.ParentId.HasValue, basePieceAfter.ParentId.HasValue);
        Assert.AreEqual(basePiece.ParentId.Value, basePieceAfter.ParentId.Value);
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
                Assert.AreEqual(anchoredMetadata.Anchor, anchoredMetadataAfter.Anchor);
                break;
            case BaseModuleBuilderMetadata baseModuleMetadata when basePieceAfter.RotationMetadata.Value is BaseModuleBuilderMetadata baseModuleMetadataAfter:
                Assert.AreEqual(baseModuleMetadata.Cell, baseModuleMetadataAfter.Cell);
                Assert.AreEqual(baseModuleMetadata.Direction, baseModuleMetadataAfter.Direction);
                break;
            case CorridorBuilderMetadata corridorMetadata when basePieceAfter.RotationMetadata.Value is CorridorBuilderMetadata corridorMetadataAfter:
                Assert.AreEqual(corridorMetadata.Rotation, corridorMetadataAfter.Rotation);
                Assert.AreEqual(corridorMetadata.Position, corridorMetadataAfter.Position);
                Assert.AreEqual(corridorMetadata.HasTargetBase, corridorMetadataAfter.HasTargetBase);
                Assert.AreEqual(corridorMetadata.Cell, corridorMetadataAfter.Cell);
                break;
            case MapRoomBuilderMetadata mapRoomMetadata when basePieceAfter.RotationMetadata.Value is MapRoomBuilderMetadata mapRoomMetadataAfter:
                Assert.AreEqual(mapRoomMetadata.CellType, mapRoomMetadataAfter.CellType);
                Assert.AreEqual(mapRoomMetadata.Rotation, mapRoomMetadataAfter.Rotation);
                break;
            case null when basePieceAfter.RotationMetadata.Value is null:
                break;
            default:
                Assert.Fail($"{nameof(BasePiece)}.{nameof(BasePiece.RotationMetadata)} is not equal");
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
                Assert.Fail($"{nameof(BasePiece)}.{nameof(BasePiece.Metadata)} is not equal");
                break;
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void PlayerDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        AssertHelper.IsListEqual(worldData.PlayerData.Players.OrderBy(x => x.Id), worldDataAfter.PlayerData.Players.OrderBy(x => x.Id), (playerData, playerDataAfter) =>
        {
            Assert.AreEqual(playerData.Name, playerDataAfter.Name);

            Assert.IsTrue(playerData.UsedItems.SequenceEqual(playerDataAfter.UsedItems));
            Assert.IsTrue(playerData.QuickSlotsBinding.SequenceEqual(playerDataAfter.QuickSlotsBinding));
            AssertHelper.IsListEqual(playerData.EquippedItems.OrderBy(x => x.ItemId), playerDataAfter.EquippedItems.OrderBy(x => x.ItemId), ItemDataTest);
            AssertHelper.IsListEqual(playerData.Modules.OrderBy(x => x.ItemId), playerDataAfter.Modules.OrderBy(x => x.ItemId), ItemDataTest);

            Assert.AreEqual(playerData.Id, playerDataAfter.Id);
            Assert.AreEqual(playerData.SpawnPosition, playerDataAfter.SpawnPosition);
            Assert.AreEqual(playerData.SpawnRotation, playerDataAfter.SpawnRotation);

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

            Assert.IsTrue(playerData.CompletedGoals.SequenceEqual(playerDataAfter.CompletedGoals));

            AssertHelper.IsDictionaryEqual(playerData.PingInstancePreferences, playerDataAfter.PingInstancePreferences, (keyValuePair, keyValuePairAfter) =>
            {
                Assert.AreEqual(keyValuePair.Key, keyValuePairAfter.Key);
                Assert.AreEqual(keyValuePair.Value.Color, keyValuePairAfter.Value.Color);
                Assert.AreEqual(keyValuePair.Value.Visible, keyValuePairAfter.Value.Visible);
            });
        });
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void EntityDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        AssertHelper.IsListEqual(worldData.EntityData.Entities.OrderBy(x => x.Id), worldDataAfter.EntityData.Entities.OrderBy(x => x.Id), EntityTest);
    }

    private static void EntityTest(Entity entity, Entity entityAfter)
    {
        Assert.AreEqual(entity.Id, entityAfter.Id);
        Assert.AreEqual(entity.TechType, entityAfter.TechType);
        Assert.AreEqual(entity.ParentId, entityAfter.ParentId);

        switch (entity.Metadata)
        {
            case KeypadMetadata metadata when entityAfter.Metadata is KeypadMetadata metadataAfter:
                Assert.AreEqual(metadata.Unlocked, metadataAfter.Unlocked);
                break;
            case SealedDoorMetadata metadata when entityAfter.Metadata is SealedDoorMetadata metadataAfter:
                Assert.AreEqual(metadata.Sealed, metadataAfter.Sealed);
                Assert.AreEqual(metadata.OpenedAmount, metadataAfter.OpenedAmount);
                break;
            case PrecursorDoorwayMetadata metadata when entityAfter.Metadata is PrecursorDoorwayMetadata metadataAfter:
                Assert.AreEqual(metadata.IsOpen, metadataAfter.IsOpen);
                break;
            case PrecursorKeyTerminalMetadata metadata when entityAfter.Metadata is PrecursorKeyTerminalMetadata metadataAfter:
                Assert.AreEqual(metadata.Slotted, metadataAfter.Slotted);
                break;
            case PrecursorTeleporterActivationTerminalMetadata metadata when entityAfter.Metadata is PrecursorTeleporterActivationTerminalMetadata metadataAfter:
                Assert.AreEqual(metadata.Unlocked, metadataAfter.Unlocked);
                break;
            case StarshipDoorMetadata metadata when entityAfter.Metadata is StarshipDoorMetadata metadataAfter:
                Assert.AreEqual(metadata.DoorLocked, metadataAfter.DoorLocked);
                Assert.AreEqual(metadata.DoorOpen, metadataAfter.DoorOpen);
                break;
            case WeldableWallPanelGenericMetadata metadata when entityAfter.Metadata is WeldableWallPanelGenericMetadata metadataAfter:
                Assert.AreEqual(metadata.LiveMixInHealth, metadataAfter.LiveMixInHealth);
                break;
            case IncubatorMetadata metadata when entityAfter.Metadata is IncubatorMetadata metadataAfter:
                Assert.AreEqual(metadata.Powered, metadataAfter.Powered);
                Assert.AreEqual(metadata.Hatched, metadataAfter.Hatched);
                break;
            case EntitySignMetadata metadata when entityAfter.Metadata is EntitySignMetadata metadataAfter:
                Assert.AreEqual(metadata.Text, metadataAfter.Text);
                Assert.AreEqual(metadata.ColorIndex, metadataAfter.ColorIndex);
                Assert.AreEqual(metadata.ScaleIndex, metadataAfter.ScaleIndex);
                Assert.IsTrue(metadata.Elements.SequenceEqual(metadataAfter.Elements));
                Assert.AreEqual(metadata.Background, metadataAfter.Background);
                break;
            case ConstructorMetadata metadata when entityAfter.Metadata is ConstructorMetadata metadataAfter:
                Assert.AreEqual(metadata.Deployed, metadataAfter.Deployed);
                break;
            case FlashlightMetadata metadata when entityAfter.Metadata is FlashlightMetadata metadataAfter:
                Assert.AreEqual(metadata.On, metadataAfter.On);
                break;
            case BatteryMetadata metadata when entityAfter.Metadata is BatteryMetadata metadataAfter:
                Assert.AreEqual(metadata.Charge, metadataAfter.Charge);
                break;
            case RepairedComponentMetadata metadata when entityAfter.Metadata is RepairedComponentMetadata metadataAfter:
                Assert.AreEqual(metadata.TechType, metadataAfter.TechType);
                break;
            case PlantableMetadata metadata when entityAfter.Metadata is PlantableMetadata metadataAfter:
                Assert.AreEqual(metadata.Progress, metadataAfter.Progress);
                break;
            case CrafterMetadata metadata when entityAfter.Metadata is CrafterMetadata metadataAfter:
                Assert.AreEqual(metadata.Duration, metadataAfter.Duration);
                Assert.AreEqual(metadata.TechType, metadataAfter.TechType);
                Assert.AreEqual(metadata.StartTime, metadataAfter.StartTime);
                break;
            case null when entityAfter.Metadata is null:
                break;
            default:
                Assert.Fail($"Runtime type of {nameof(Entity)}.{nameof(Entity.Metadata)} is not equal");
                break;
        }

        switch (entity)
        {
            case WorldEntity worldEntity when entityAfter is WorldEntity worldEntityAfter:
                Assert.AreEqual(worldEntity.Transform.LocalPosition, worldEntityAfter.Transform.LocalPosition);
                Assert.AreEqual(worldEntity.Transform.LocalRotation, worldEntityAfter.Transform.LocalRotation);
                Assert.AreEqual(worldEntity.Transform.LocalScale, worldEntityAfter.Transform.LocalScale);
                Assert.AreEqual(worldEntity.Level, worldEntityAfter.Level);
                Assert.AreEqual(worldEntity.ClassId, worldEntityAfter.ClassId);
                Assert.AreEqual(worldEntity.SpawnedByServer, worldEntityAfter.SpawnedByServer);
                Assert.AreEqual(worldEntity.WaterParkId, worldEntityAfter.WaterParkId);
                Assert.AreEqual(worldEntity.ExistsInGlobalRoot, worldEntityAfter.ExistsInGlobalRoot);

                switch (worldEntity)
                {
                    case EscapePodWorldEntity escapePodWorldEntity when worldEntityAfter is EscapePodWorldEntity escapePodWorldEntityAfter:
                        Assert.AreEqual(escapePodWorldEntity.Damaged, escapePodWorldEntityAfter.Damaged);
                        Assert.IsTrue(escapePodWorldEntity.Players.SequenceEqual(escapePodWorldEntityAfter.Players));
                        break;
                    default:
                        Assert.AreEqual(worldEntity.GetType(), worldEntityAfter.GetType());
                        break;
                }

                break;
            case PrefabChildEntity prefabChildEntity when entityAfter is PrefabChildEntity prefabChildEntityAfter:
                Assert.AreEqual(prefabChildEntity.ComponentIndex, prefabChildEntityAfter.ComponentIndex);
                Assert.AreEqual(prefabChildEntity.ClassId, prefabChildEntityAfter.ClassId);
                break;
            case PrefabPlaceholderEntity prefabPlaceholderEntity when entityAfter is PrefabPlaceholderEntity prefabPlaceholderEntityAfter:
                Assert.AreEqual(prefabPlaceholderEntity.ClassId, prefabPlaceholderEntityAfter.ClassId);
                break;
            case InventoryEntity inventoryEntity when entityAfter is InventoryEntity inventoryEntityAfter:
                Assert.AreEqual(inventoryEntity.ComponentIndex, inventoryEntityAfter.ComponentIndex);
                break;
            case InventoryItemEntity inventoryItemEntity when entityAfter is InventoryItemEntity inventoryItemEntityAfter:
                Assert.AreEqual(inventoryItemEntity.ClassId, inventoryItemEntityAfter.ClassId);
                break;
            default:
                Assert.Fail($"Runtime type of {nameof(Entity)} is not equal");
                break;
        }

        AssertHelper.IsListEqual(entity.ChildEntities.OrderBy(x => x.Id), entityAfter.ChildEntities.OrderBy(x => x.Id), EntityTest);
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
                        new PrefabChildEntity(new NitroxId(), "pretty class id", new NitroxTechType("Fabricator"), 1, new CrafterMetadata(new NitroxTechType("FilteredWater"), 100, 10), new NitroxId()),
                        new PrefabPlaceholderEntity(new NitroxId(), new NitroxTechType("Bulkhead"), new NitroxId()),
                        new WorldEntity(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 1, "PeeperClass", false, new NitroxId(), null, false, new NitroxId()),
                        new PlaceholderGroupWorldEntity(new WorldEntity(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, NitroxTechType.None, 1, "Wreck1", false, new NitroxId(), null, false, new NitroxId()), new List<PrefabPlaceholderEntity>()
                        {
                            new(new NitroxId(), new NitroxTechType("Door"), new NitroxId())
                        }),
                        new EscapePodWorldEntity(NitroxVector3.One, new NitroxId(), new RepairedComponentMetadata(new NitroxTechType("Radio"))),
                        new InventoryEntity(1, new NitroxId(), new NitroxTechType("planterbox"), null, new NitroxId(), new List<Entity>()
                        {
                            new InventoryItemEntity(new NitroxId(), "classId", new NitroxTechType("bluepalmseed"), new PlantableMetadata(0.5f), new NitroxId(), new List<Entity>())
                        })
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
                    StoryTiming = new StoryTimingData()
                    {
                        ElapsedTime = 10,
                        AuroraExplosionTime = 10000,
                        AuroraWarningTime = 20
                    },
                },
                InventoryData = new InventoryData()
                {
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
