using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test;
using Nitrox.Test.Helper;
using Nitrox.Test.Helper.Faker;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxServer_Subnautica;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Serialization.World;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxServer.Serialization;

[TestClass]
public class WorldPersistenceTest
{
    private static readonly string tempSaveFilePath = Path.Combine(Path.GetTempPath(), "NitroxTestTempDir");
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

        worldData = new NitroxAutoFaker<PersistedWorldData>().Generate();

        for (int index = 0; index < ServerSerializers.Length; index++)
        {
            //Checking saving
            worldPersistence.UpdateSerializer(ServerSerializers[index]);
            Assert.IsTrue(worldPersistence.Save(worldData, tempSaveFilePath), $"Saving normal world failed while using {ServerSerializers[index]}.");

            //Checking loading
            WorldsDataAfter[index] = worldPersistence.LoadDataFromPath(tempSaveFilePath);
            Assert.IsNotNull(WorldsDataAfter[index], $"Loading saved world failed while using {ServerSerializers[index]}.");
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void WorldDataTest(PersistedWorldData worldDataAfter, string serializerName)
    {
        Assert.IsTrue(worldData.WorldData.ParsedBatchCells.SequenceEqual(worldDataAfter.WorldData.ParsedBatchCells));
        Assert.AreEqual(worldData.WorldData.Seed, worldDataAfter.WorldData.Seed);

        PDAStateTest(worldData.WorldData.GameData.PDAState, worldDataAfter.WorldData.GameData.PDAState);
        StoryGoalTest(worldData.WorldData.GameData.StoryGoals, worldDataAfter.WorldData.GameData.StoryGoals);
        StoryTimingTest(worldData.WorldData.GameData.StoryTiming, worldDataAfter.WorldData.GameData.StoryTiming);
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

    private static void PDAStateTest(PDAStateData pdaState, PDAStateData pdaStateAfter)
    {
        Assert.IsTrue(pdaState.KnownTechTypes.SequenceEqual(pdaStateAfter.KnownTechTypes));
        Assert.IsTrue(pdaState.AnalyzedTechTypes.SequenceEqual(pdaStateAfter.AnalyzedTechTypes));
        AssertHelper.IsListEqual(pdaState.PdaLog.OrderBy(x => x.Key), pdaStateAfter.PdaLog.OrderBy(x => x.Key), (entry, entryAfter) =>
        {
            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Timestamp, entryAfter.Timestamp);
        });
        Assert.IsTrue(pdaState.EncyclopediaEntries.SequenceEqual(pdaStateAfter.EncyclopediaEntries));
        Assert.IsTrue(pdaState.ScannerFragments.SequenceEqual(pdaStateAfter.ScannerFragments));
        AssertHelper.IsListEqual(pdaState.ScannerPartial.OrderBy(x => x.TechType.Name), pdaStateAfter.ScannerPartial.OrderBy(x => x.TechType.Name), (entry, entryAfter) =>
        {
            Assert.AreEqual(entry.TechType, entryAfter.TechType);
            Assert.AreEqual(entry.Unlocked, entryAfter.Unlocked);
        });

        Assert.IsTrue(pdaState.ScannerComplete.SequenceEqual(pdaStateAfter.ScannerComplete));
    }

    private static void StoryGoalTest(StoryGoalData storyGoal, StoryGoalData storyGoalAfter)
    {
        Assert.IsTrue(storyGoal.CompletedGoals.SequenceEqual(storyGoalAfter.CompletedGoals));
        Assert.IsTrue(storyGoal.RadioQueue.SequenceEqual(storyGoalAfter.RadioQueue));
        Assert.IsTrue(storyGoal.GoalUnlocks.SequenceEqual(storyGoalAfter.GoalUnlocks));
        AssertHelper.IsListEqual(storyGoal.ScheduledGoals.OrderBy(x => x.GoalKey), storyGoalAfter.ScheduledGoals.OrderBy(x => x.GoalKey), (scheduledGoal, scheduledGoalAfter) =>
        {
            Assert.AreEqual(scheduledGoal.TimeExecute, scheduledGoalAfter.TimeExecute);
            Assert.AreEqual(scheduledGoal.GoalKey, scheduledGoalAfter.GoalKey);
            Assert.AreEqual(scheduledGoal.GoalType, scheduledGoalAfter.GoalType);
        });
    }

    private static void StoryTimingTest(StoryTimingData storyTiming, StoryTimingData storyTimingAfter)
    {
        Assert.AreEqual(storyTiming.ElapsedSeconds, storyTimingAfter.ElapsedSeconds);
        Assert.AreEqual(storyTiming.AuroraCountdownTime, storyTimingAfter.AuroraCountdownTime);
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
            Assert.IsTrue(playerData.QuickSlotsBindingIds.SequenceEqual(playerDataAfter.QuickSlotsBindingIds));
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

            Assert.IsTrue(playerData.PersonalCompletedGoalsWithTimestamp.SequenceEqual(playerDataAfter.PersonalCompletedGoalsWithTimestamp));

            AssertHelper.IsDictionaryEqual(playerData.PlayerPreferences.PingPreferences, playerDataAfter.PlayerPreferences.PingPreferences, (keyValuePair, keyValuePairAfter) =>
            {
                Assert.AreEqual(keyValuePair.Key, keyValuePairAfter.Key);
                Assert.AreEqual(keyValuePair.Value.Color, keyValuePairAfter.Value.Color);
                Assert.AreEqual(keyValuePair.Value.Visible, keyValuePairAfter.Value.Visible);
            });
            Assert.IsTrue(playerData.PlayerPreferences.PinnedTechTypes.SequenceEqual(playerDataAfter.PlayerPreferences.PinnedTechTypes));
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
            case PrecursorTeleporterMetadata metadata when entityAfter.Metadata is PrecursorTeleporterMetadata metadataAfter:
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
            case CrafterMetadata metadata when entityAfter.Metadata is CrafterMetadata metadataAfter:
                Assert.AreEqual(metadata.TechType, metadataAfter.TechType);
                Assert.AreEqual(metadata.StartTime, metadataAfter.StartTime);
                Assert.AreEqual(metadata.Duration, metadataAfter.Duration);
                break;
            case PlantableMetadata metadata when entityAfter.Metadata is PlantableMetadata metadataAfter:
                Assert.AreEqual(metadata.Progress, metadataAfter.Progress);
                break;
            case CyclopsMetadata metadata when entityAfter.Metadata is CyclopsMetadata metadataAfter:
                Assert.AreEqual(metadata.SilentRunningOn, metadataAfter.SilentRunningOn);
                Assert.AreEqual(metadata.ShieldOn, metadataAfter.ShieldOn);
                Assert.AreEqual(metadata.SonarOn, metadataAfter.SonarOn);
                Assert.AreEqual(metadata.EngineOn, metadataAfter.EngineOn);
                Assert.AreEqual(metadata.EngineMode, metadataAfter.EngineMode);
                Assert.AreEqual(metadata.Health, metadataAfter.Health);
                break;
            case SeamothMetadata metadata when entityAfter.Metadata is SeamothMetadata metadataAfter:
                Assert.AreEqual(metadata.LightsOn, metadataAfter.LightsOn);
                Assert.AreEqual(metadata.Health, metadataAfter.Health);
                break;
            case SubNameInputMetadata metadata when entityAfter.Metadata is SubNameInputMetadata metadataAfter:
                Assert.AreEqual(metadata.Name, metadataAfter.Name);
                Assert.IsTrue(metadata.Colors.SequenceEqual(metadataAfter.Colors));
                break;
            case RocketMetadata metadata when entityAfter.Metadata is RocketMetadata metadataAfter:
                Assert.AreEqual(metadata.CurrentStage, metadataAfter.CurrentStage);
                Assert.AreEqual(metadata.LastStageTransitionTime, metadataAfter.LastStageTransitionTime);
                Assert.AreEqual(metadata.ElevatorState, metadataAfter.ElevatorState);
                Assert.AreEqual(metadata.ElevatorPosition, metadataAfter.ElevatorPosition);
                Assert.IsTrue(metadata.PreflightChecks.SequenceEqual(metadataAfter.PreflightChecks));
                break;
            case CyclopsLightingMetadata metadata when entityAfter.Metadata is CyclopsLightingMetadata metadataAfter:
                Assert.AreEqual(metadata.FloodLightsOn, metadataAfter.FloodLightsOn);
                Assert.AreEqual(metadata.InternalLightsOn, metadataAfter.InternalLightsOn);
                break;
            case FireExtinguisherHolderMetadata metadata when entityAfter.Metadata is FireExtinguisherHolderMetadata metadataAfter:
                Assert.AreEqual(metadata.HasExtinguisher, metadataAfter.HasExtinguisher);
                Assert.AreEqual(metadata.Fuel, metadataAfter.Fuel);
                break;
            case PlayerMetadata metadata when entityAfter.Metadata is PlayerMetadata metadataAfter:
                AssertHelper.IsListEqual(metadata.EquippedItems.OrderBy(x => x.Id), metadataAfter.EquippedItems.OrderBy(x => x.Id), (equippedItem, equippedItemAfter) =>
                {
                    Assert.AreEqual(equippedItem.Id, equippedItemAfter.Id);
                    Assert.AreEqual(equippedItem.Slot, equippedItemAfter.Slot);
                    Assert.AreEqual(equippedItem.TechType, equippedItemAfter.TechType);
                });
                break;
            default:
                Assert.Fail($"Runtime type of {nameof(Entity)}.{nameof(Entity.Metadata)} is not equal: {entity.Metadata?.GetType().Name} - {entityAfter.Metadata?.GetType().Name}");
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

                if (worldEntity.GetType() != worldEntityAfter.GetType())
                {
                    Assert.Fail($"Runtime type of {nameof(WorldEntity)} is not equal: {worldEntity.GetType().Name} - {worldEntityAfter.GetType().Name}");
                }
                else if (worldEntity.GetType() != typeof(WorldEntity))
                {
                    switch (worldEntity)
                    {
                        case PlaceholderGroupWorldEntity _ when worldEntityAfter is PlaceholderGroupWorldEntity _:
                            break;
                        case EscapePodWorldEntity escapePodWorldEntity when worldEntityAfter is EscapePodWorldEntity escapePodWorldEntityAfter:
                            Assert.AreEqual(escapePodWorldEntity.Damaged, escapePodWorldEntityAfter.Damaged);
                            Assert.IsTrue(escapePodWorldEntity.Players.SequenceEqual(escapePodWorldEntityAfter.Players));
                            break;
                        case PlayerWorldEntity _ when worldEntityAfter is PlayerWorldEntity _:
                            break;
                        case VehicleWorldEntity vehicleWorldEntity when worldEntityAfter is VehicleWorldEntity vehicleWorldEntityAfter:
                            Assert.AreEqual(vehicleWorldEntity.SpawnerId, vehicleWorldEntityAfter.SpawnerId);
                            Assert.AreEqual(vehicleWorldEntity.ConstructionTime, vehicleWorldEntityAfter.ConstructionTime);
                            break;
                        case CellRootEntity _ when worldEntityAfter is CellRootEntity _:
                            break;
                        default:
                            Assert.Fail($"Runtime type of {nameof(WorldEntity)} is not equal even after the check: {worldEntity.GetType().Name} - {worldEntityAfter.GetType().Name}");
                            break;
                    }
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
            case PathBasedChildEntity pathBasedChildEntity when entityAfter is PathBasedChildEntity pathBasedChildEntityAfter:
                Assert.AreEqual(pathBasedChildEntity.Path, pathBasedChildEntityAfter.Path);
                break;
            case InstalledBatteryEntity _ when entityAfter is InstalledBatteryEntity _:
                break;
            case InstalledModuleEntity installedModuleEntity when entityAfter is InstalledModuleEntity installedModuleEntityAfter:
                Assert.AreEqual(installedModuleEntity.Slot, installedModuleEntityAfter.Slot);
                Assert.AreEqual(installedModuleEntity.ClassId, installedModuleEntityAfter.ClassId);
                break;
            default:
                Assert.Fail($"Runtime type of {nameof(Entity)} is not equal: {entity.GetType().Name} - {entityAfter.GetType().Name}");
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
}

[AttributeUsage(AttributeTargets.Method)]
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
