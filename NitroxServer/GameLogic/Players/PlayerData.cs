using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using ProtoBufNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract]
    public class PlayerData
    {
        [ProtoMember(1)]
        public Dictionary<string, PersistedPlayerData> SerializablePlayersByPlayerName
        {
            get
            {
                lock (playersByPlayerName)
                {
                    return new Dictionary<string, PersistedPlayerData>(playersByPlayerName);
                }
            }
            set { playersByPlayerName = value; }
        }

        [ProtoMember(2)]
        public Dictionary<string, EquippedItemData> SerializableModules
        {
            get
            {
                lock (ModulesItemsByGuid)
                {
                    return new Dictionary<string, EquippedItemData>(ModulesItemsByGuid);
                }
            }
            set { ModulesItemsByGuid = value; }
        }

        public Dictionary<string, EquippedItemData> ModulesItemsByGuid = new Dictionary<string, EquippedItemData>();

        private Dictionary<string, PersistedPlayerData> playersByPlayerName = new Dictionary<string, PersistedPlayerData>();

        public void AddEquipment(string playerName, EquippedItemData equippedItem)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerData = playersByPlayerName[playerName];
                playerData.EquippedItemsByGuid.Add(equippedItem.Guid, equippedItem);
            }
        }

        public string Inventory(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = null;

                if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }

                if (string.IsNullOrEmpty(playerPersistedData.PlayerInventoryGuid))
                {
                    return playerPersistedData.PlayerInventoryGuid = Guid.NewGuid().ToString();
                }
                else
                {
                    return playerPersistedData.PlayerInventoryGuid;
                }
            }
        }

        public Vector3 PlayerSpawn(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = null;

                if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }
                return playerPersistedData.PlayerSpawnData;
            }
        }

        public void UpdatePlayerSpawn(string playerName, Vector3 position)
        {
            lock (playersByPlayerName)
            {
                playersByPlayerName[playerName].PlayerSpawnData = position;
            }
        }

        public void PlayerStats(string playerName, PlayerStats statsData)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = null;

                if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }

                playersByPlayerName[playerName].CurrentStats = new PlayerStatsData(statsData.Oxygen, statsData.MaxOxygen, statsData.Health, statsData.Food, statsData.Water);
            }
        }
        public PlayerStatsData Stats(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = null;

                if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }

                return playersByPlayerName[playerName].CurrentStats;
            }
        }

        
        public void RemoveEquipment(string playerName, string guid)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerData = playersByPlayerName[playerName];
                playerData.EquippedItemsByGuid.Remove(guid);
            }
        }

        public List<EquippedItemData> GetEquippedItemsForInitialSync(string playerName)
        {
            PersistedPlayerData playerPersistedData = null;

            lock (playersByPlayerName)
            {
                if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }

                List<EquippedItemData> ItemData = new List<EquippedItemData>(playerPersistedData.EquippedItemsByGuid.Values);
                ItemData.AddRange((new List<EquippedItemData>(ModulesItemsByGuid.Values)));
                return ItemData;
            }
        }

        public void AddModule(EquippedItemData equippedmodule)
        {
            lock (ModulesItemsByGuid)
            {
                ModulesItemsByGuid.Add(equippedmodule.Guid, equippedmodule);
            }
        }
        public void RemoveModule(string guid)
        {
            lock (ModulesItemsByGuid)
            {
                ModulesItemsByGuid.Remove(guid);
            }
        }

        [ProtoContract]
        public class PersistedPlayerData
        {
            [ProtoMember(1)]
            public string PlayerName { get; set; }

            [ProtoMember(2)]
            public Dictionary<string, EquippedItemData> EquippedItemsByGuid { get; set; } = new Dictionary<string, EquippedItemData>();

            [ProtoMember(3)]
            public string PlayerInventoryGuid { get; set; }

            [ProtoMember(4)]
            public Vector3 PlayerSpawnData { get; set; }

            [ProtoMember(5)]
            public PlayerStatsData CurrentStats { get; set; }

            public PersistedPlayerData()
            {
                // Constructor for serialization purposes
            }

            public PersistedPlayerData(string playerName)
            {
                PlayerName = playerName;
            }
        }

    }
}
