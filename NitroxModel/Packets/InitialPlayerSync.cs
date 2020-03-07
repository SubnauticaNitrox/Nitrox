using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public List<EscapePodModel> EscapePodsData { get; }
        public NitroxId AssignedEscapePodId;
        public List<EquippedItemData> EquippedItems { get; }
        public List<EquippedItemData> Modules { get; }
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public List<ItemData> StorageSlots { get; }
        public NitroxId PlayerGameObjectId { get; }
        public bool FirstTimeConnecting { get; }
        public InitialPdaData PDAData { get; }
        public InitialStoryGoalData StoryGoalData { get; }
        public Vector3 PlayerSpawnData { get; }
        public Optional<NitroxId> PlayerSubRootId { get; }
        public PlayerStatsData PlayerStatsData { get; }
        public List<InitialRemotePlayerData> RemotePlayerData { get; }
        public List<Entity> GlobalRootEntities { get; }
        public string GameMode { get; }
        public Perms Permissions { get; }

        public InitialPlayerSync(NitroxId playerGameObjectId, bool firstTimeConnecting, List<EscapePodModel> escapePodsData, NitroxId assignedEscapePodId, List<EquippedItemData> equipment, List<EquippedItemData> modules, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems, List<ItemData> storageSlots, InitialPdaData pdaData, InitialStoryGoalData storyGoalData, Vector3 playerSpawnData, Optional<NitroxId> playerSubRootId, PlayerStatsData playerStatsData, List<InitialRemotePlayerData> remotePlayerData, List<Entity> globalRootEntities, string gameMode, Perms perms)
        {
            EscapePodsData = escapePodsData;
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equipment;
            Modules = modules;
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
            StorageSlots = storageSlots;
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData;
            GlobalRootEntities = globalRootEntities;
            GameMode = gameMode;
            Permissions = perms;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - EquippedItems: " + EquippedItems + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + " PDAData: " + PDAData + " StoryGoalData: " + StoryGoalData + "]";
        }
    }
}
