using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public List<EscapePodModel> EscapePodsData { get; }
        public string AssignedEscapePodGuid;
        public List<EquippedItemData> EquippedItems { get; }
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public string PlayerGuid { get; }
        public bool FirstTimeConnecting { get; }
        public InitialPdaData PDAData { get; }
        public Vector3 PlayerSpawnData { get; }
        public Optional<string> PlayerSubRootGuid { get; }
        public PlayerStatsData PlayerStatsData { get; }
        public List<InitialRemotePlayerData> RemotePlayerData { get; }
        public List<Entity> GlobalRootEntities { get; }

        public InitialPlayerSync(string playerGuid, bool firstTimeConnecting, List<EscapePodModel> escapePodsData, string assignedEscapePodGuid, List<EquippedItemData> equipment, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems, InitialPdaData pdaData, Vector3 playerSpawnData, Optional<string> playerSubRootGuid, PlayerStatsData playerStatsData, List<InitialRemotePlayerData> remotePlayerData, List<Entity> globalRootEntities)
        {
            EscapePodsData = escapePodsData;
            AssignedEscapePodGuid = assignedEscapePodGuid;
            PlayerGuid = playerGuid;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equipment;
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
            PDAData = pdaData;
            PlayerSpawnData = playerSpawnData;
            PlayerSubRootGuid = playerSubRootGuid;
            PlayerStatsData = playerStatsData;
            RemotePlayerData = remotePlayerData;
            GlobalRootEntities = globalRootEntities;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - EquippedItems: " + EquippedItems + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + " PDAData: " + PDAData + "]";
        }
    }
}
