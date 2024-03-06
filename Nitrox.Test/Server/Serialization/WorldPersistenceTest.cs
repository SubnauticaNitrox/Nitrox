using Nitrox.Test;
using Nitrox.Test.Helper.Faker;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;
using NitroxServer.Serialization.World;
using NitroxServer_Subnautica;

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
                Assert.AreEqual(metadata.Name, metadataAfter.Name);
                Assert.IsTrue(metadata.Colors.SequenceEqual(metadataAfter.Colors));
                break;
            case ExosuitMetadata metadata when entityAfter.Metadata is ExosuitMetadata metadataAfter:
                Assert.AreEqual(metadata.Health, metadataAfter.Health);
                Assert.AreEqual(metadata.Name, metadataAfter.Name);
                Assert.IsTrue(metadata.Colors.SequenceEqual(metadataAfter.Colors));
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
            case GhostMetadata ghostMetadata when entityAfter.Metadata is GhostMetadata ghostMetadataAfter:
                Assert.AreEqual(ghostMetadata.TargetOffset, ghostMetadataAfter.TargetOffset);

                if (ghostMetadata.GetType() != ghostMetadataAfter.GetType())
                {
                    Assert.Fail($"Runtime type of {nameof(GhostMetadata)} in {nameof(Entity)}.{nameof(Entity.Metadata)} is not equal: {ghostMetadata.GetType().Name} - {ghostMetadataAfter.GetType().Name}");
                }

                switch (ghostMetadata)
                {
                    case BaseAnchoredCellGhostMetadata metadata when ghostMetadataAfter is BaseAnchoredCellGhostMetadata metadataAfter:
                        Assert.AreEqual(metadata.AnchoredCell, metadataAfter.AnchoredCell);
                        break;
                    case BaseAnchoredFaceGhostMetadata metadata when ghostMetadataAfter is BaseAnchoredFaceGhostMetadata metadataAfter:
                        Assert.AreEqual(metadata.AnchoredFace, metadataAfter.AnchoredFace);
                        break;
                    case BaseDeconstructableGhostMetadata metadata when ghostMetadataAfter is BaseDeconstructableGhostMetadata metadataAfter:
                        Assert.AreEqual(metadata.ModuleFace, metadataAfter.ModuleFace);
                        Assert.AreEqual(metadata.ClassId, metadataAfter.ClassId);
                        break;
                }

                break;
            case WaterParkCreatureMetadata metadata when entityAfter.Metadata is WaterParkCreatureMetadata metadataAfter:
                Assert.AreEqual(metadata.Age, metadataAfter.Age);
                Assert.AreEqual(metadata.MatureTime, metadataAfter.MatureTime);
                Assert.AreEqual(metadata.TimeNextBreed, metadataAfter.TimeNextBreed);
                Assert.AreEqual(metadata.BornInside, metadataAfter.BornInside);
                break;
            case FlareMetadata metadata when entityAfter.Metadata is FlareMetadata metadataAfter:
                Assert.AreEqual(metadata.EnergyLeft, metadataAfter.EnergyLeft);
                Assert.AreEqual(metadata.HasBeenThrown, metadataAfter.HasBeenThrown);
                Assert.AreEqual(metadata.FlareActivateTime, metadataAfter.FlareActivateTime);
                break;
            case BeaconMetadata metadata when entityAfter.Metadata is BeaconMetadata metadataAfter:
                Assert.AreEqual(metadata.Label, metadataAfter.Label);
                break;
            case RadiationMetadata metadata when entityAfter.Metadata is RadiationMetadata metadataAfter:
                Assert.AreEqual(metadata.Health, metadataAfter.Health);
                Assert.AreEqual(metadata.FixRealTime, metadataAfter.FixRealTime);
                break;
            case CrashHomeMetadata metadata when entityAfter.Metadata is CrashHomeMetadata metadataAfter:
                Assert.AreEqual(metadata.SpawnTime, metadataAfter.SpawnTime);
                break;
            case EatableMetadata metadata when entityAfter.Metadata is EatableMetadata metadataAfter:
                Assert.AreEqual(metadata.TimeDecayStart, metadataAfter.TimeDecayStart);
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

                if (worldEntity.GetType() != worldEntityAfter.GetType())
                {
                    Assert.Fail($"Runtime type of {nameof(WorldEntity)} is not equal: {worldEntity.GetType().Name} - {worldEntityAfter.GetType().Name}");
                }
                else if (worldEntity.GetType() != typeof(WorldEntity))
                {
                    switch (worldEntity)
                    {
                        case PlaceholderGroupWorldEntity placeholderGroupWorldEntity when worldEntityAfter is PlaceholderGroupWorldEntity placeholderGroupWorldEntityAfter:
                            Assert.AreEqual(placeholderGroupWorldEntity.ComponentIndex, placeholderGroupWorldEntityAfter.ComponentIndex);
                            break;
                        case CellRootEntity _ when worldEntityAfter is CellRootEntity _:
                            break;
                        case PlacedWorldEntity _ when worldEntityAfter is PlacedWorldEntity _:
                            break;
                        case OxygenPipeEntity oxygenPipeEntity when worldEntityAfter is OxygenPipeEntity oxygenPipeEntityAfter:
                            Assert.AreEqual(oxygenPipeEntity.ParentPipeId, oxygenPipeEntityAfter.ParentPipeId);
                            Assert.AreEqual(oxygenPipeEntity.RootPipeId, oxygenPipeEntityAfter.RootPipeId);
                            Assert.AreEqual(oxygenPipeEntity.ParentPosition, oxygenPipeEntityAfter.ParentPosition);
                            break;
                        case PrefabPlaceholderEntity prefabPlaceholderEntity when entityAfter is PrefabPlaceholderEntity prefabPlaceholderEntityAfter:
                            Assert.AreEqual(prefabPlaceholderEntity.ComponentIndex, prefabPlaceholderEntityAfter.ComponentIndex);
                            break;
                        case SerializedWorldEntity serializedWorldEntity when entityAfter is SerializedWorldEntity serializedWorldEntityAfter:
                            Assert.AreEqual(serializedWorldEntity.AbsoluteEntityCell, serializedWorldEntityAfter.AbsoluteEntityCell);
                            AssertHelper.IsListEqual(serializedWorldEntity.Components.OrderBy(c => c.GetHashCode()), serializedWorldEntityAfter.Components.OrderBy(c => c.GetHashCode()), (SerializedComponent c1, SerializedComponent c2) => c1.Equals(c2));
                            Assert.AreEqual(serializedWorldEntity.Layer, serializedWorldEntityAfter.Layer);
                            Assert.AreEqual(serializedWorldEntity.BatchId, serializedWorldEntityAfter.BatchId);
                            Assert.AreEqual(serializedWorldEntity.CellId, serializedWorldEntityAfter.CellId);
                            break;
                        case GeyserWorldEntity geyserEntity when entityAfter is GeyserWorldEntity geyserEntityAfter:
                            Assert.AreEqual(geyserEntity.RandomIntervalVarianceMultiplier, geyserEntityAfter.RandomIntervalVarianceMultiplier);
                            Assert.AreEqual(geyserEntity.StartEruptTime, geyserEntityAfter.StartEruptTime);
                            break;
                        case ReefbackEntity reefbackEntity when entityAfter is ReefbackEntity reefbackEntityAfter:
                            Assert.AreEqual(reefbackEntity.GrassIndex, reefbackEntityAfter.GrassIndex);
                            Assert.AreEqual(reefbackEntity.OriginalPosition, reefbackEntityAfter.OriginalPosition);
                            break;
                        case ReefbackChildEntity reefbackChildEntity when entityAfter is ReefbackChildEntity reefbackChildEntityAfter:
                            Assert.AreEqual(reefbackChildEntity.Type, reefbackChildEntityAfter.Type);
                            break;
                        case CreatureRespawnEntity creatureRespawnEntity when entityAfter is CreatureRespawnEntity creatureRespawnEntityAfter:
                            Assert.AreEqual(creatureRespawnEntity.SpawnTime, creatureRespawnEntityAfter.SpawnTime);
                            Assert.AreEqual(creatureRespawnEntity.RespawnTechType, creatureRespawnEntityAfter.RespawnTechType);
                            Assert.IsTrue(creatureRespawnEntity.AddComponents.SequenceEqual(creatureRespawnEntityAfter.AddComponents));
                            break;
                        case GlobalRootEntity globalRootEntity when worldEntityAfter is GlobalRootEntity globalRootEntityAfter:
                            if (globalRootEntity.GetType() != typeof(GlobalRootEntity))
                            {
                                switch (globalRootEntity)
                                {
                                    case BuildEntity buildEntity when globalRootEntityAfter is BuildEntity buildEntityAfter:
                                        Assert.AreEqual(buildEntity.BaseData, buildEntityAfter.BaseData);
                                        break;
                                    case EscapePodWorldEntity escapePodWorldEntity when globalRootEntityAfter is EscapePodWorldEntity escapePodWorldEntityAfter:
                                        Assert.AreEqual(escapePodWorldEntity.Damaged, escapePodWorldEntityAfter.Damaged);
                                        Assert.IsTrue(escapePodWorldEntity.Players.SequenceEqual(escapePodWorldEntityAfter.Players));
                                        break;
                                    case InteriorPieceEntity interiorPieceEntity when globalRootEntityAfter is InteriorPieceEntity interiorPieceEntityAfter:
                                        Assert.AreEqual(interiorPieceEntity.BaseFace, interiorPieceEntityAfter.BaseFace);
                                        break;
                                    case MapRoomEntity mapRoomEntity when globalRootEntityAfter is MapRoomEntity mapRoomEntityAfter:
                                        Assert.AreEqual(mapRoomEntity.Cell, mapRoomEntityAfter.Cell);
                                        break;
                                    case ModuleEntity moduleEntity when globalRootEntityAfter is ModuleEntity moduleEntityAfter:
                                        Assert.AreEqual(moduleEntity.ConstructedAmount, moduleEntityAfter.ConstructedAmount);
                                        Assert.AreEqual(moduleEntity.IsInside, moduleEntityAfter.IsInside);

                                        if (moduleEntity.GetType() != moduleEntityAfter.GetType())
                                        {
                                            Assert.Fail($"Runtime type of {nameof(ModuleEntity)} is not equal: {moduleEntity.GetType().Name} - {moduleEntityAfter.GetType().Name}");
                                        }

                                        switch (moduleEntity)
                                        {
                                            case GhostEntity ghostEntity when moduleEntityAfter is GhostEntity ghostEntityAfter:
                                                Assert.AreEqual(ghostEntity.BaseFace, ghostEntityAfter.BaseFace);
                                                Assert.AreEqual(ghostEntity.BaseData, ghostEntityAfter.BaseData);
                                                break;
                                        }

                                        break;
                                    case MoonpoolEntity moonpoolEntity when globalRootEntityAfter is MoonpoolEntity moonpoolEntityAfter:
                                        Assert.AreEqual(moonpoolEntity.Cell, moonpoolEntityAfter.Cell);
                                        break;
                                    case PlanterEntity _ when globalRootEntityAfter is PlanterEntity:
                                        break;
                                    case PlayerWorldEntity _ when globalRootEntityAfter is PlayerWorldEntity:
                                        break;
                                    case VehicleWorldEntity vehicleWorldEntity when globalRootEntityAfter is VehicleWorldEntity vehicleWorldEntityAfter:
                                        Assert.AreEqual(vehicleWorldEntity.SpawnerId, vehicleWorldEntityAfter.SpawnerId);
                                        Assert.AreEqual(vehicleWorldEntity.ConstructionTime, vehicleWorldEntityAfter.ConstructionTime);
                                        break;
                                    case RadiationLeakEntity radiationLeakEntity when globalRootEntityAfter is RadiationLeakEntity radiationLeakEntityAfter:
                                        Assert.AreEqual(radiationLeakEntity.ObjectIndex, radiationLeakEntityAfter.ObjectIndex);
                                        break;
                                    default:
                                        Assert.Fail($"Runtime type of {nameof(GlobalRootEntity)} is not equal even after the check: {worldEntity.GetType().Name} - {globalRootEntityAfter.GetType().Name}");
                                        break;
                                }
                            }
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
            case BaseLeakEntity baseLeakEntity when entityAfter is BaseLeakEntity baseLeakEntityAfter:
                Assert.AreEqual(baseLeakEntity.Health, baseLeakEntityAfter.Health);
                Assert.AreEqual(baseLeakEntity.RelativeCell, baseLeakEntityAfter.RelativeCell);
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
