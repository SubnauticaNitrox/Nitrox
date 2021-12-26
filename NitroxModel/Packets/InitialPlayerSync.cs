using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class InitialPlayerSync : Packet
    {
        [Index(0)]
        public virtual NitroxId AssignedEscapePodId { get; protected set; }
        [Index(1)]
        public virtual List<EscapePodModel> EscapePodsData { get; protected set; }
        [Index(2)]
        public virtual List<EquippedItemData> EquippedItems { get; protected set; }
        [Index(3)]
        public virtual List<EquippedItemData> Modules { get; protected set; }
        [Index(4)]
        public virtual List<BasePiece> BasePieces { get; protected set; }
        [Index(5)]
        public virtual List<VehicleModel> Vehicles { get; protected set; }
        [Index(6)]
        public virtual List<ItemData> InventoryItems { get; protected set; }
        [Index(7)]
        public virtual List<ItemData> StorageSlotItems { get; protected set; }
        [Index(8)]
        public virtual List<NitroxTechType> UsedItems { get; protected set; }
        [Index(9)]
        public virtual List<string> QuickSlotsBinding { get; protected set; }
        [Index(10)]
        public virtual NitroxId PlayerGameObjectId { get; protected set; }
        [Index(11)]
        public virtual bool FirstTimeConnecting { get; protected set; }
        [Index(12)]
        public virtual InitialPDAData PDAData { get; protected set; }
        [Index(13)]
        public virtual InitialStoryGoalData StoryGoalData { get; protected set; }
        [Index(14)]
        public virtual NitroxVector3 PlayerSpawnData { get; protected set; }
        [Index(15)]
        public virtual Optional<NitroxId> PlayerSubRootId { get; protected set; }
        [Index(16)]
        public virtual PlayerStatsData PlayerStatsData { get; protected set; }
        [Index(17)]
        public virtual List<InitialRemotePlayerData> RemotePlayerData { get; protected set; }
        [Index(18)]
        public virtual List<Entity> GlobalRootEntities { get; protected set; }
        [Index(19)]
        public virtual List<NitroxId> InitialSimulationOwnerships { get; protected set; }
        [Index(20)]
        public virtual ServerGameMode GameMode { get; protected set; }
        [Index(21)]
        public virtual Perms Permissions { get; protected set; }

        private InitialPlayerSync() { }

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
            NitroxVector3 playerSpawnData,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            IEnumerable<InitialRemotePlayerData> remotePlayerData,
            IEnumerable<Entity> globalRootEntities,
            IEnumerable<NitroxId> initialSimulationOwnerships,
            ServerGameMode gameMode,
            Perms perms)
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
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData.ToList();
            GlobalRootEntities = globalRootEntities.ToList();
            InitialSimulationOwnerships = initialSimulationOwnerships.ToList();
            GameMode = gameMode;
            Permissions = perms;
        }
    }
}
