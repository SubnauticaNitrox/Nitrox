using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet, IShortString
    {
        public NitroxId AssignedEscapePodId;
        public List<EscapePodModel> EscapePodsData { get; }
        public List<EquippedItemData> EquippedItems { get; }
        public List<EquippedItemData> Modules { get; }
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public List<ItemData> StorageSlots { get; }
        public NitroxId PlayerGameObjectId { get; }
        public bool FirstTimeConnecting { get; }
        public InitialPDAData PDAData { get; }
        public InitialStoryGoalData StoryGoalData { get; }
        public Vector3 PlayerSpawnData { get; }
        public Optional<NitroxId> PlayerSubRootId { get; }
        public PlayerStatsData PlayerStatsData { get; }
        public List<InitialRemotePlayerData> RemotePlayerData { get; }
        public List<Entity> GlobalRootEntities { get; }
        public string GameMode { get; }
        public Perms Permissions { get; }

        public InitialPlayerSync(NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            IEnumerable<EscapePodModel> escapePodsData,
            NitroxId assignedEscapePodId,
            IEnumerable<EquippedItemData> equipment,
            IEnumerable<EquippedItemData> modules,
            IEnumerable<BasePiece> basePieces,
            IEnumerable<VehicleModel> vehicles,
            IEnumerable<ItemData> inventoryItems,
            IEnumerable<ItemData> storageSlots,
            InitialPDAData pdaData,
            InitialStoryGoalData storyGoalData,
            Vector3 playerSpawnData,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            IEnumerable<InitialRemotePlayerData> remotePlayerData,
            IEnumerable<Entity> globalRootEntities,
            string gameMode,
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
            StorageSlots = storageSlots.ToList();
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData.ToList();
            GlobalRootEntities = globalRootEntities.ToList();
            GameMode = gameMode;
            Permissions = perms;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - EquippedItems: " + EquippedItems + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + " PDAData: " + PDAData + " StoryGoalData: " + StoryGoalData + "]";
        }

        public string ToShortString()
        {
            return $"Equipped items count: {EquippedItems.Count}\n" +
                $"Base pieces count: {BasePieces.Count}\n" +
                $"Vehicles count: {Vehicles.Count}\n" +
                $"Inventory items count: {InventoryItems.Count}";
        }
    }
}
