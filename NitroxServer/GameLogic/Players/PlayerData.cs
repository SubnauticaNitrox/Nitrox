using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System;
using System.Collections.Generic;

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

                return new List<EquippedItemData>(playerPersistedData.EquippedItemsByGuid.Values);
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
