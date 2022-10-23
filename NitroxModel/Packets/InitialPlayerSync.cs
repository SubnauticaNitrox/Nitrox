using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public NitroxId AssignedEscapePodId;
        public List<EscapePodModel> EscapePodsData { get; }
        public List<EquippedItemData> EquippedItems { get; }
        public List<EquippedItemData> Modules { get; }
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public List<ItemData> StorageSlotItems { get; }
        public List<NitroxTechType> UsedItems { get; }
        public List<string> QuickSlotsBinding { get; }
        public NitroxId PlayerGameObjectId { get; }
        public bool FirstTimeConnecting { get; }
        public InitialPDAData PDAData { get; }
        public InitialStoryGoalData StoryGoalData { get; }
        public ICollection<string> CompletedGoals { get; }
        public NitroxVector3 PlayerSpawnData { get; }
        public Optional<NitroxId> PlayerSubRootId { get; }
        public PlayerStatsData PlayerStatsData { get; }
        public List<InitialRemotePlayerData> RemotePlayerData { get; }
        public List<Entity> GlobalRootEntities { get; }
        public List<NitroxId> InitialSimulationOwnerships { get; }
        public ServerGameMode GameMode { get; }
        public Perms Permissions { get; }
        public Dictionary<string, PingInstancePreference> Preferences { get; }

        public InitialPlayerSync(NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            IEnumerable<EscapePodModel> escapePodsData,
            NitroxId assignedEscapePodId,
            IEnumerable<EquippedItemData> equipment,
            IEnumerable<EquippedItemData> modules,
            IEnumerable<BasePiece> basePieces,
            IEnumerable<VehicleModel> vehicles,
            IEnumerable<ItemData> inventoryItems,
            IEnumerable<ItemData> storageSlotItems,
            IEnumerable<NitroxTechType> usedItems,
            IEnumerable<string> quickSlotsBinding,
            InitialPDAData pdaData,
            InitialStoryGoalData storyGoalData,
            ICollection<string> completedGoals,
            NitroxVector3 playerSpawnData,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            IEnumerable<InitialRemotePlayerData> remotePlayerData,
            IEnumerable<Entity> globalRootEntities,
            IEnumerable<NitroxId> initialSimulationOwnerships,
            ServerGameMode gameMode,
            Perms perms,
            Dictionary<string, PingInstancePreference> preferences)
        {
            EscapePodsData = escapePodsData.ToList();
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equipment.ToList();
            Modules = modules.ToList();
            BasePieces = basePieces.ToList();
            Vehicles = vehicles.ToList();
            InventoryItems = inventoryItems.ToList();
            StorageSlotItems = storageSlotItems.ToList();
            UsedItems = usedItems.ToList();
            QuickSlotsBinding = quickSlotsBinding.ToList();
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            CompletedGoals = completedGoals;
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData.ToList();
            GlobalRootEntities = globalRootEntities.ToList();
            InitialSimulationOwnerships = initialSimulationOwnerships.ToList();
            GameMode = gameMode;
            Permissions = perms;
            Preferences = preferences;
        }

        /// <remarks>Used for deserialization</remarks>
        public InitialPlayerSync(NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            List<EscapePodModel> escapePodsData,
            NitroxId assignedEscapePodId,
            List<EquippedItemData> equippedItems,
            List<EquippedItemData> modules,
            List<BasePiece> basePieces,
            List<VehicleModel> vehicles,
            List<ItemData> inventoryItems,
            List<ItemData> storageSlotItems,
            List<NitroxTechType> usedItems,
            List<string> quickSlotsBinding,
            InitialPDAData pdaData,
            InitialStoryGoalData storyGoalData,
            ICollection<string> completedGoals,
            NitroxVector3 playerSpawnData,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            List<InitialRemotePlayerData> remotePlayerData,
            List<Entity> globalRootEntities,
            List<NitroxId> initialSimulationOwnerships,
            ServerGameMode gameMode,
            Perms permissions,
            Dictionary<string, PingInstancePreference> preferences)
        {
            EscapePodsData = escapePodsData;
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equippedItems;
            Modules = modules;
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
            StorageSlotItems = storageSlotItems;
            UsedItems = usedItems;
            QuickSlotsBinding = quickSlotsBinding;
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            CompletedGoals = completedGoals;
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData;
            GlobalRootEntities = globalRootEntities;
            InitialSimulationOwnerships = initialSimulationOwnerships;
            GameMode = gameMode;
            Permissions = permissions;
            Preferences = preferences;
        }
    }
}
