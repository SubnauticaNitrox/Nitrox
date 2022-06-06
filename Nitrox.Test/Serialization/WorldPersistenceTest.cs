using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.SaveData;
using NitroxServer_Subnautica;
using NitroxTest;

namespace NitroxServer.Serialization;

[TestClass]
public class WorldPersistenceTest
{
    private static string tempSaveFilePath;
    private static PersistedSaveData worldData;

    public static PersistedSaveData[] WorldsDataAfter;
    public static IServerSerializer[] ServerSerializers;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar(), new TestAutoFacRegistrar());
        NitroxServiceLocator.BeginNewLifetimeScope();

        WorldPersistence worldPersistence = NitroxServiceLocator.LocateService<WorldPersistence>();
        ServerSerializers = NitroxServiceLocator.LocateService<IServerSerializer[]>();
        WorldsDataAfter = new PersistedSaveData[ServerSerializers.Length];
        tempSaveFilePath = Path.Combine(Path.GetTempPath(), "NitroxTestTempDir");

        worldData = GeneratePersistedSaveData();
        World world = worldPersistence.CreateWorld(worldData, ServerGameMode.CREATIVE);
        world.EventTriggerer.ResetWorld();

        for (int index = 0; index < ServerSerializers.Length; index++)
        {
            //Checking saving
            worldPersistence.serializer = ServerSerializers[index];
            Assert.IsTrue(worldPersistence.Save(world, tempSaveFilePath), $"Saving normal world failed while using {ServerSerializers[index]}.");
            Assert.IsFalse(worldPersistence.Save(null, tempSaveFilePath), $"Saving null world worked while using {ServerSerializers[index]}.");

            //Checking loading
            Optional<World> worldAfter = worldPersistence.LoadFromFile(tempSaveFilePath);
            Assert.IsTrue(worldAfter.HasValue, $"Loading saved world failed while using {ServerSerializers[index]}.");
            worldAfter.Value.EventTriggerer.ResetWorld();
            WorldsDataAfter[index] = PersistedSaveData.From(worldAfter.Value);
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void WorldDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        Assert.IsTrue(worldData.WorldData.ParsedBatchCells.SequenceEqual(worldDataAfter.WorldData.ParsedBatchCells));
        Assert.AreEqual(worldData.WorldData.Seed, worldDataAfter.WorldData.Seed);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void VehicleDataTest(PersistedSaveData worldDataAfter, string serializerName)
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

            Assert.AreEqual(vehicleModel.InteractiveChildIdentifiers.Count, vehicleModelAfter.InteractiveChildIdentifiers.Count);
            for (int identifierIndex = 0; identifierIndex < vehicleModel.InteractiveChildIdentifiers.Count; identifierIndex++)
            {
                Assert.AreEqual(vehicleModel.InteractiveChildIdentifiers[identifierIndex].Id, vehicleModelAfter.InteractiveChildIdentifiers[identifierIndex].Id);
                Assert.AreEqual(vehicleModel.InteractiveChildIdentifiers[identifierIndex].GameObjectNamePath, vehicleModelAfter.InteractiveChildIdentifiers[identifierIndex].GameObjectNamePath);
            }

            Assert.AreEqual(vehicleModel.DockingBayId, vehicleModelAfter.DockingBayId);
            Assert.AreEqual(vehicleModel.Name, vehicleModelAfter.Name);
            Assert.IsTrue(vehicleModel.HSB.SequenceEqual(vehicleModelAfter.HSB));
            Assert.AreEqual(vehicleModel.Health, vehicleModelAfter.Health);

            switch (vehicleModel)
            {
                case SeamothModel seamoth when vehicleModelAfter is SeamothModel seamothAfter:
                    Assert.AreEqual(seamoth.LightOn, seamothAfter.LightOn);
                    break;
                case CyclopsModel cyclops when vehicleModelAfter is CyclopsModel cyclopsAfter:
                    Assert.AreEqual(cyclops.FloodLightsOn, cyclopsAfter.FloodLightsOn);
                    Assert.AreEqual(cyclops.InternalLightsOn, cyclopsAfter.InternalLightsOn);
                    Assert.AreEqual(cyclops.SilentRunningOn, cyclopsAfter.SilentRunningOn);
                    Assert.AreEqual(cyclops.ShieldOn, cyclopsAfter.ShieldOn);
                    Assert.AreEqual(cyclops.SonarOn, cyclopsAfter.SonarOn);
                    Assert.AreEqual(cyclops.EngineState, cyclopsAfter.EngineState);
                    Assert.AreEqual(cyclops.EngineMode, cyclopsAfter.EngineMode);
                    break;
                case ExosuitModel exosuit when vehicleModelAfter is ExosuitModel exosuitAfter:
                    Assert.AreEqual(exosuit.LeftArmId, exosuitAfter.LeftArmId);
                    Assert.AreEqual(exosuit.RightArmId, exosuitAfter.RightArmId);
                    break;
                case NeptuneRocketModel rocket when vehicleModelAfter is NeptuneRocketModel rocketAfter:
                    Assert.AreEqual(rocket.CurrentStage, rocketAfter.CurrentStage);
                    Assert.AreEqual(rocket.ElevatorUp, rocketAfter.ElevatorUp);
                    Assert.IsTrue(rocket.PreflightChecks.SequenceEqual(rocketAfter.PreflightChecks));
                    break;
                default:
                    Assert.Fail($"VehicleModel was serialized as an abstract class");
                    break;
            }
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void InventoryDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.WorldData.InventoryData.InventoryItems.Count, worldDataAfter.WorldData.InventoryData.InventoryItems.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.InventoryItems.Count; index++)
        {
            ItemDataTest(worldData.WorldData.InventoryData.InventoryItems[index], worldDataAfter.WorldData.InventoryData.InventoryItems[index]);
        }

        Assert.AreEqual(worldData.WorldData.InventoryData.StorageSlotItems.Count, worldDataAfter.WorldData.InventoryData.StorageSlotItems.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.StorageSlotItems.Count; index++)
        {
            ItemDataTest(worldData.WorldData.InventoryData.StorageSlotItems[index], worldDataAfter.WorldData.InventoryData.StorageSlotItems[index]);
        }

        Assert.AreEqual(worldData.WorldData.InventoryData.Modules.Count, worldDataAfter.WorldData.InventoryData.Modules.Count);
        for (int index = 0; index < worldData.WorldData.InventoryData.Modules.Count; index++)
        {
            ItemDataTest(worldData.WorldData.InventoryData.Modules[index], worldDataAfter.WorldData.InventoryData.Modules[index]);
        }
    }

    private void ItemDataTest(ItemData itemData, ItemData itemDataAfter)
    {
        Assert.AreEqual(itemData.ContainerId, itemDataAfter.ContainerId);
        Assert.AreEqual(itemData.ItemId, itemDataAfter.ItemId);
        Assert.IsTrue(itemData.SerializedData.SequenceEqual(itemDataAfter.SerializedData));

        switch (itemData)
        {
            case EquippedItemData equippedItem when itemDataAfter is EquippedItemData equippedItemAfter:
                Assert.AreEqual(equippedItem.Slot, equippedItemAfter.Slot);
                Assert.AreEqual(equippedItem.TechType, equippedItemAfter.TechType);
                break;
            case PlantableItemData plantableItem when itemDataAfter is PlantableItemData plantableItemAfter:
                Assert.AreEqual(plantableItem.PlantedGameTime, plantableItemAfter.PlantedGameTime);
                break;
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void GameDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        GameData gameData = worldData.WorldData.GameData;
        GameData gameDataAfter = worldDataAfter.WorldData.GameData;

        // WorldData.GameData.PDAState
        Assert.IsTrue(gameData.PDAState.UnlockedTechTypes.SequenceEqual(gameDataAfter.PDAState.UnlockedTechTypes));

        Assert.AreEqual(gameData.PDAState.PartiallyUnlockedByTechType.Count, gameDataAfter.PDAState.PartiallyUnlockedByTechType.Count);
        for (int index = 0; index < gameData.PDAState.PartiallyUnlockedByTechType.Count; index++)
        {
            KeyValuePair<NitroxTechType, PDAEntry> entry = gameData.PDAState.PartiallyUnlockedByTechType.ElementAt(index);
            KeyValuePair<NitroxTechType, PDAEntry> entryAfter = gameDataAfter.PDAState.PartiallyUnlockedByTechType.ElementAt(index);

            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType);
            Assert.AreEqual(entry.Value.Progress, entryAfter.Value.Progress);
            Assert.AreEqual(entry.Value.Unlocked, entryAfter.Value.Unlocked);
        }

        Assert.IsTrue(gameData.PDAState.KnownTechTypes.SequenceEqual(gameDataAfter.PDAState.KnownTechTypes));
        Assert.IsTrue(gameData.PDAState.AnalyzedTechTypes.SequenceEqual(gameDataAfter.PDAState.AnalyzedTechTypes));
        Assert.IsTrue(gameData.PDAState.EncyclopediaEntries.SequenceEqual(gameDataAfter.PDAState.EncyclopediaEntries));

        Assert.AreEqual(gameData.PDAState.PdaLog.Count, gameDataAfter.PDAState.PdaLog.Count);
        for (int index = 0; index < gameData.PDAState.PdaLog.Count; index++)
        {
            PDALogEntry logEntry = gameData.PDAState.PdaLog[index];
            PDALogEntry logEntryAfter = gameDataAfter.PDAState.PdaLog[index];

            Assert.AreEqual(logEntry.Key, logEntryAfter.Key);
            Assert.AreEqual(logEntry.Timestamp, logEntryAfter.Timestamp);
        }

        Assert.AreEqual(gameData.PDAState.CachedProgress.Count, gameDataAfter.PDAState.CachedProgress.Count);
        for (int index = 0; index < gameData.PDAState.CachedProgress.Count; index++)
        {
            KeyValuePair<NitroxTechType, PDAProgressEntry> entry = gameData.PDAState.CachedProgress.ElementAt(index);
            KeyValuePair<NitroxTechType, PDAProgressEntry> entryAfter = gameDataAfter.PDAState.CachedProgress.ElementAt(index);

            Assert.AreEqual(entry.Key, entryAfter.Key);
            Assert.AreEqual(entry.Value.TechType, entryAfter.Value.TechType);

            Assert.AreEqual(entry.Value.Entries.Count, entryAfter.Value.Entries.Count);
            for (int subIndex = 0; subIndex < entry.Value.Entries.Count; subIndex++)
            {
                KeyValuePair<NitroxId, float> subEntry = entry.Value.Entries.ElementAt(subIndex);
                KeyValuePair<NitroxId, float> subEntryAfter = entryAfter.Value.Entries.ElementAt(subIndex);

                Assert.AreEqual(subEntry.Key, subEntryAfter.Key);
                Assert.AreEqual(subEntry.Value, subEntryAfter.Value);
            }
        }

        // WorldData.GameData.StoryGoals
        Assert.IsTrue(gameData.StoryGoals.CompletedGoals.SequenceEqual(gameDataAfter.StoryGoals.CompletedGoals));
        Assert.IsTrue(gameData.StoryGoals.RadioQueue.SequenceEqual(gameDataAfter.StoryGoals.RadioQueue));
        Assert.IsTrue(gameData.StoryGoals.GoalUnlocks.SequenceEqual(gameDataAfter.StoryGoals.GoalUnlocks));

        Assert.AreEqual(gameData.StoryGoals.ScheduledGoals.Count, gameDataAfter.StoryGoals.ScheduledGoals.Count);
        for (int index = 0; index < gameData.PDAState.PdaLog.Count; index++)
        {
            NitroxScheduledGoal scheduledGoal = gameData.StoryGoals.ScheduledGoals[index];
            NitroxScheduledGoal scheduledGoalAfter = gameDataAfter.StoryGoals.ScheduledGoals[index];

            Assert.AreEqual(scheduledGoal.TimeExecute, scheduledGoalAfter.TimeExecute);
            Assert.AreEqual(scheduledGoal.GoalKey, scheduledGoalAfter.GoalKey);
            Assert.AreEqual(scheduledGoal.GoalType, scheduledGoalAfter.GoalType);
        }

        // WorldData.GameData.StoryTiming
        Assert.AreEqual(gameData.StoryTiming.ElapsedTime, gameDataAfter.StoryTiming.ElapsedTime);
        Assert.AreEqual(gameData.StoryTiming.AuroraExplosionTime, gameDataAfter.StoryTiming.AuroraExplosionTime);
        Assert.AreEqual(gameData.StoryTiming.AuroraWarningTime, gameDataAfter.StoryTiming.AuroraWarningTime);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void EscapePodDataTest(PersistedSaveData worldDataAfter, string serializerName)
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
    public void BaseDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.BaseData.PartiallyConstructedPieces.Count, worldDataAfter.BaseData.PartiallyConstructedPieces.Count);
        for (int index = 0; index < worldData.BaseData.CompletedBasePieceHistory.Count; index++)
        {
            BasePieceTest(worldData.BaseData.CompletedBasePieceHistory[index], worldDataAfter.BaseData.CompletedBasePieceHistory[index]);
        }

        Assert.AreEqual(worldData.BaseData.PartiallyConstructedPieces.Count, worldDataAfter.BaseData.PartiallyConstructedPieces.Count);
        for (int index = 0; index < worldData.BaseData.PartiallyConstructedPieces.Count; index++)
        {
            BasePieceTest(worldData.BaseData.PartiallyConstructedPieces[index], worldDataAfter.BaseData.PartiallyConstructedPieces[index]);
        }
    }

    private void BasePieceTest(BasePiece basePiece, BasePiece basePieceAfter)
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
                Assert.AreEqual(corridorMetadata.Position, corridorMetadataAfter.Position);
                Assert.AreEqual(corridorMetadata.Rotation, corridorMetadataAfter.Rotation);
                Assert.AreEqual(corridorMetadata.HasTargetBase, corridorMetadataAfter.HasTargetBase);
                Assert.AreEqual(corridorMetadata.Cell, corridorMetadataAfter.Cell);
                break;
            case MapRoomBuilderMetadata mapRoomMetadata when basePieceAfter.RotationMetadata.Value is MapRoomBuilderMetadata mapRoomMetadataAfter:
                Assert.AreEqual(mapRoomMetadata.CellType, mapRoomMetadataAfter.CellType);
                Assert.AreEqual(mapRoomMetadata.ConnectionMask, mapRoomMetadataAfter.ConnectionMask);
                break;
            case null when basePieceAfter.RotationMetadata.Value is null:
                break;
            default:
                Assert.Fail("BasePiece.RotationMetadata type is not equal");
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
                Assert.Fail("BasePiece.Metadata type is not equal");
                break;
        }

        Assert.AreEqual(basePiece.BuildIndex, basePieceAfter.BuildIndex);
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void PlayerDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        Assert.AreEqual(worldData.PlayerData.Players.Count, worldDataAfter.PlayerData.Players.Count);
        for (int playerIndex = 0; playerIndex < worldData.PlayerData.Players.Count; playerIndex++)
        {
            PersistedPlayerData playerData = worldData.PlayerData.Players[playerIndex];
            PersistedPlayerData playerDataAfter = worldDataAfter.PlayerData.Players[playerIndex];

            Assert.AreEqual(playerData.Name, playerDataAfter.Name);
            Assert.IsTrue(playerData.UsedItems.SequenceEqual(playerDataAfter.UsedItems));
            Assert.IsTrue(playerData.QuickSlotsBinding.SequenceEqual(playerDataAfter.QuickSlotsBinding));

            Assert.AreEqual(playerData.EquippedItems.Count, playerDataAfter.EquippedItems.Count);
            for (int index = 0; index < playerData.EquippedItems.Count; index++)
            {
                ItemDataTest(playerData.EquippedItems[index], playerDataAfter.EquippedItems[index]);
            }

            Assert.AreEqual(playerData.Modules.Count, playerDataAfter.Modules.Count);
            for (int index = 0; index < playerData.Modules.Count; index++)
            {
                ItemDataTest(playerData.Modules[index], playerDataAfter.Modules[index]);
            }

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
            Assert.IsTrue(playerData.CompletedGoals.SequenceEqual(playerDataAfter.CompletedGoals));
        }
    }

    [DataTestMethod, DynamicWorldDataAfter]
    public void EntityDataTest(PersistedSaveData worldDataAfter, string serializerName)
    {
        for (int index = 0; index < worldData.EntityData.Entities.Count; index++)
        {
            Entity entity = worldData.EntityData.Entities[index];
            Entity entityAfter = worldDataAfter.EntityData.Entities[index];

            Assert.AreEqual(entity.Transform.LocalPosition, entityAfter.Transform.LocalPosition);
            Assert.AreEqual(entity.Transform.LocalRotation, entityAfter.Transform.LocalRotation);
            Assert.AreEqual(entity.Transform.LocalScale, entityAfter.Transform.LocalScale);

            Assert.AreEqual(entity.TechType, entityAfter.TechType);
            Assert.AreEqual(entity.Id, entityAfter.Id);
            Assert.AreEqual(entity.Level, entityAfter.Level);
            Assert.AreEqual(entity.ClassId, entityAfter.ClassId);
            Assert.AreEqual(entity.SpawnedByServer, entityAfter.SpawnedByServer);
            Assert.AreEqual(entity.WaterParkId, entityAfter.WaterParkId);
            Assert.IsTrue(entity.SerializedGameObject.SequenceEqual(entityAfter.SerializedGameObject));
            Assert.AreEqual(entity.ExistsInGlobalRoot, entityAfter.ExistsInGlobalRoot);
            Assert.AreEqual(entity.ParentId, entityAfter.ParentId);

            switch (entity.Metadata)
            {
                case EntitySignMetadata signMetadata when entityAfter.Metadata is EntitySignMetadata signMetadataAfter:
                    Assert.AreEqual(signMetadata.Text, signMetadataAfter.Text);
                    Assert.AreEqual(signMetadata.ColorIndex, signMetadataAfter.ColorIndex);
                    Assert.AreEqual(signMetadata.ScaleIndex, signMetadataAfter.ScaleIndex);
                    Assert.IsTrue(signMetadata.Elements.SequenceEqual(signMetadataAfter.Elements));
                    Assert.AreEqual(signMetadata.Background, signMetadataAfter.Background);
                    break;
                case IncubatorMetadata incubatorMetadata when entityAfter.Metadata is IncubatorMetadata incubatorMetadataAfter:
                    Assert.AreEqual(incubatorMetadata.Powered, incubatorMetadataAfter.Powered);
                    Assert.AreEqual(incubatorMetadata.Hatched, incubatorMetadataAfter.Hatched);
                    break;
                case KeypadMetadata keypadMetadata when entityAfter.Metadata is KeypadMetadata keypadMetadataAfter:
                    Assert.AreEqual(keypadMetadata.Unlocked, keypadMetadataAfter.Unlocked);
                    break;
                case PrecursorDoorwayMetadata precursorDoorwayMetadata when entityAfter.Metadata is PrecursorDoorwayMetadata precursorDoorwayMetadataAfter:
                    Assert.AreEqual(precursorDoorwayMetadata.IsOpen, precursorDoorwayMetadataAfter.IsOpen);
                    break;
                case PrecursorKeyTerminalMetadata precursorKeyTerminalMetadata when entityAfter.Metadata is PrecursorKeyTerminalMetadata precursorKeyTerminalMetadataAfter:
                    Assert.AreEqual(precursorKeyTerminalMetadata.Slotted, precursorKeyTerminalMetadataAfter.Slotted);
                    break;
                case PrecursorTeleporterActivationTerminalMetadata precursorTeleporterActivationTerminalMetadata when entityAfter.Metadata is PrecursorTeleporterActivationTerminalMetadata precursorTeleporterActivationTerminalMetadataAfter:
                    Assert.AreEqual(precursorTeleporterActivationTerminalMetadata.Unlocked, precursorTeleporterActivationTerminalMetadataAfter.Unlocked);
                    break;
                case PrecursorTeleporterMetadata precursorTeleporterMetadata when entityAfter.Metadata is PrecursorTeleporterMetadata precursorTeleporterMetadataAfter:
                    Assert.AreEqual(precursorTeleporterMetadata.IsOpen, precursorTeleporterMetadataAfter.IsOpen);
                    break;
                case SealedDoorMetadata sealedDoorMetadata when entityAfter.Metadata is SealedDoorMetadata sealedDoorMetadataAfter:
                    Assert.AreEqual(sealedDoorMetadata.IsSealed, sealedDoorMetadataAfter.IsSealed);
                    Assert.AreEqual(sealedDoorMetadata.OpenedAmount, sealedDoorMetadataAfter.OpenedAmount);
                    break;
                case StarshipDoorMetadata starshipDoorMetadata when entityAfter.Metadata is StarshipDoorMetadata starshipDoorMetadataAfter:
                    Assert.AreEqual(starshipDoorMetadata.DoorLocked, starshipDoorMetadataAfter.DoorLocked);
                    Assert.AreEqual(starshipDoorMetadata.DoorOpen, starshipDoorMetadataAfter.DoorOpen);
                    break;
                case WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadata when entityAfter.Metadata is WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadataAfter:
                    Assert.AreEqual(weldableWallPanelGenericMetadata.LiveMixInHealth, weldableWallPanelGenericMetadataAfter.LiveMixInHealth);
                    break;
                case null when entityAfter.Metadata is null:
                    break;
                default:
                    Assert.Fail("Entity.Metadata type is not equal");
                    break;
            }

            Assert.AreEqual(entity.ExistingGameObjectChildIndex, entityAfter.ExistingGameObjectChildIndex);
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

    private static PersistedSaveData GeneratePersistedSaveData()
    {
        return new PersistedSaveData()
        {
            BaseData =
                new BaseData()
                {
                    CompletedBasePieceHistory =
                        new List<BasePiece>()
                        {
                            new(new NitroxId(), NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.Zero, NitroxQuaternion.Identity, new NitroxTechType("BasePiece1"), Optional<NitroxId>.Of(new NitroxId()), false, 1.0f, true, 1,
                                Optional.Empty, Optional<BasePieceMetadata>.Of(new SignMetadata("ExampleText", 1, 2, new[] { true, false }, true)))
                        },
                    PartiallyConstructedPieces = new List<BasePiece>()
                    {
                        new(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                            Optional<BuilderMetadata>.Of(new AnchoredFaceBuilderMetadata(new NitroxInt3(1, 2, 3), 1, 2))),
                        new(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                            Optional<BuilderMetadata>.Of(new BaseModuleBuilderMetadata(new NitroxInt3(1, 2, 3), 1))),
                        new(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                            Optional<BuilderMetadata>.Of(new CorridorBuilderMetadata(new NitroxVector3(1, 2, 3), 2, false, default))),
                        new(new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, NitroxQuaternion.Identity, new NitroxTechType("BasePiece2"), Optional.Empty, false,
                            Optional<BuilderMetadata>.Of(new MapRoomBuilderMetadata(0x20, 2)))
                    }
                },
            EntityData =
                new EntityData()
                {
                    Entities = new List<Entity>()
                    {
                        new(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Peeper"), 2, "PeeperClass", false, new NitroxId(), new byte[] { 0x10, 0x14, 0x0, 0x2, 0x2, 0x2, 0x2 }, false, new NitroxId())
                        {
                            ParentId = new NitroxId(), ExistingGameObjectChildIndex = 3
                        },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Sign"), 1, "SignClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId())
                        {
                            Metadata = new EntitySignMetadata("SignText", 2, 1, new[] { true, false }, true)
                        },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Incubator"), 1, "IncubatorClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId())
                        {
                            Metadata = new IncubatorMetadata(true, false)
                        },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("Keypad"), 1, "KeypadClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId())
                        {
                            Metadata = new KeypadMetadata(true)
                        },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("PrecursorDoorway"), 1, "PrecursorDoorwayClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true,
                            new NitroxId()) { Metadata = new PrecursorDoorwayMetadata(true) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("PrecursorKeyTerminal"), 1, "PrecursorKeyTerminalClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 },
                            true, new NitroxId()) { Metadata = new PrecursorKeyTerminalMetadata(true) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("PrecursorTeleporterActivationTerminal"), 1, "PrecursorTeleporterActivationTerminalClass", false, new NitroxId(),
                            new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId()) { Metadata = new PrecursorTeleporterActivationTerminalMetadata(true) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("PrecursorTeleporter"), 1, "PrecursorTeleporterClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 },
                            true, new NitroxId()) { Metadata = new PrecursorTeleporterMetadata(true) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("SealedDoor"), 1, "SealedDoorClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true,
                            new NitroxId()) { Metadata = new SealedDoorMetadata(true, 0.75f) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("StarshipDoor"), 1, "StarshipDoorClass", false, new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true,
                            new NitroxId()) { Metadata = new StarshipDoorMetadata(true, false) },
                        new(NitroxVector3.One, NitroxQuaternion.Identity, NitroxVector3.One, new NitroxTechType("WeldableWallPanelGeneric"), 1, "WeldableWallPanelGenericClass", false, new NitroxId(),
                            new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, true, new NitroxId()) { Metadata = new WeldableWallPanelGenericMetadata(0.33f) }
                    }
                },
            PlayerData = new PlayerData()
            {
                Players = new List<PersistedPlayerData>()
                {
                    new()
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
                        Modules = new List<EquippedItemData>(0)
                    },
                    new()
                    {
                        NitroxId = new NitroxId(),
                        Id = 2,
                        Name = "Test2",
                        IsPermaDeath = true,
                        Permissions = Perms.PLAYER,
                        SpawnPosition = NitroxVector3.One,
                        SubRootId = new NitroxId(),
                        CurrentStats = new PlayerStatsData(40, 40, 30, 29, 28, 0),
                        UsedItems = new List<NitroxTechType> { new("Knife"), new("Flashlight") },
                        QuickSlotsBinding = new List<string> { "Test1", "Test2" },
                        EquippedItems = new List<EquippedItemData>
                        {
                            new(new NitroxId(), new NitroxId(), new byte[] { 0x30, 0x40 }, "Slot3", new NitroxTechType("Flashlight")), new(new NitroxId(), new NitroxId(), new byte[] { 0x50, 0x9D }, "Slot4", new NitroxTechType("Knife"))
                        },
                        Modules = new List<EquippedItemData>() { new(new NitroxId(), new NitroxId(), new byte[] { 0x35, 0xD0 }, "Module1", new NitroxTechType("Compass")) }
                    }
                }
            },
            WorldData = new WorldData()
            {
                EscapePodData = new EscapePodData()
                {
                    EscapePods = new List<EscapePodModel>()
                    {
                        new()
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
                        UnlockedTechTypes = { new NitroxTechType("base") },
                        PartiallyUnlockedByTechType = new ThreadSafeDictionary<NitroxTechType, PDAEntry> { { new NitroxTechType("Moonpool"), new PDAEntry(new NitroxTechType("Moonpool"), 50f, 2) } },
                        KnownTechTypes = { new NitroxTechType("Knife") },
                        AnalyzedTechTypes = { new NitroxTechType("HotKnife") },
                        EncyclopediaEntries = { "TestEntry1", "TestEntry2" },
                        PdaLog = { new PDALogEntry("key1", 1.1234f) },
                        CachedProgress = new ThreadSafeDictionary<NitroxTechType, PDAProgressEntry>
                        {
                            { new NitroxTechType("MoonpoolStation"), new PDAProgressEntry(new NitroxTechType("MoonpoolStation"), new Dictionary<NitroxId, float> { { new NitroxId(), 0.25f } }) }
                        }
                    },
                    StoryGoals = new StoryGoalData()
                    {
                        CompletedGoals = { "Goal1", "Goal2" },
                        GoalUnlocks = { "Goal3", "Goal4" },
                        RadioQueue = { "Queue1" },
                        ScheduledGoals = new ThreadSafeList<NitroxScheduledGoal> { NitroxScheduledGoal.From(1002f, "goalKey1", "super Type") }
                    },
                    StoryTiming = new StoryTimingData()
                    {
                        AuroraExplosionTime = 10000,
                        ElapsedTime = 10,
                        AuroraWarningTime = 99
                    },
                },
                InventoryData = new InventoryData()
                {
                    InventoryItems = new List<ItemData>() { new(new NitroxId(), new NitroxId(), new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }) },
                    StorageSlotItems = new List<ItemData>() { new(new NitroxId(), new NitroxId(), new byte[] { 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 }) },
                    Modules = new List<EquippedItemData>() { new(new NitroxId(), new NitroxId(), new byte[] { 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21 }, "Slot1", new NitroxTechType("")) }
                },
                ParsedBatchCells = new List<NitroxInt3>() { new(10, 1, 10), new(15, 4, 12) },
                VehicleData = new VehicleData()
                {
                    Vehicles = new List<VehicleModel>()
                    {
                        new SeamothModel(new NitroxTechType("Seamoth"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, new List<InteractiveChildObjectIdentifier> { new(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem") },
                                         Optional<NitroxId>.Of(new NitroxId()), "Super Duper Seamoth", new[] { NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One }, 4) { LightOn = false },
                        new CyclopsModel(new NitroxTechType("Cyclops"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, new List<InteractiveChildObjectIdentifier>(),
                                         Optional<NitroxId>.Of(new NitroxId()), "Super Duper Cyclops", new[] { NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One }, 33)
                        {
                            FloodLightsOn = false,
                            InternalLightsOn = false,
                            SilentRunningOn = true,
                            ShieldOn = true,
                            SonarOn = true,
                            EngineState = true,
                            EngineMode = CyclopsMotorMode.CyclopsMotorModes.Flank
                        },
                        new ExosuitModel(new NitroxTechType("Cyclops"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, new List<InteractiveChildObjectIdentifier> { new(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem") },
                                         Optional<NitroxId>.Of(new NitroxId()), "Claw 3000", new[] { NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One }, 100),
                        new NeptuneRocketModel(new NitroxTechType("Seamoth"), new NitroxId(), NitroxVector3.One, NitroxQuaternion.Identity, new List<InteractiveChildObjectIdentifier> { new(new NitroxId(), "/BlablaScene/SomeBlablaContainer/BlaItem") },
                                               Optional<NitroxId>.Of(new NitroxId()), "Mega Death Rocket", new[] { NitroxVector3.Zero, NitroxVector3.One, NitroxVector3.One }, 14)
                        {
                            CurrentStage = 2,
                            ElevatorUp = true,
                            PreflightChecks = new ThreadSafeList<PreflightCheck> { PreflightCheck.Hydraulics, PreflightCheck.TimeCapsule }
                        }
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
