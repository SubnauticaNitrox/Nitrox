using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using ProtoBufNet;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;
using System.Linq;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract]
    public class PlayerData
    {
        public const long VERSION = 1;

        [ProtoMember(1)]
        public Dictionary<string, PersistedPlayerData> SerializablePlayersByPlayerName
        {
            get
            {
                lock (playersByPlayerName)
                {
                    serializablePlayersByPlayerName = new Dictionary<string, PersistedPlayerData>(playersByPlayerName);
                    return serializablePlayersByPlayerName;
                }
            }
            set { playersByPlayerName = value; }
        }

        private Dictionary<string, PersistedPlayerData> serializablePlayersByPlayerName = new Dictionary<string, PersistedPlayerData>();

        [ProtoMember(2)]
        public Dictionary<NitroxId, EquippedItemData> SerializableModules
        {
            get
            {
                lock (ModulesItemsById)
                {
                    serializableModules = new Dictionary<NitroxId, EquippedItemData>(ModulesItemsById);
                    return serializableModules;
                }
            }
            set { ModulesItemsById = value; }
        }

        Dictionary<NitroxId, EquippedItemData> serializableModules = new Dictionary<NitroxId, EquippedItemData>();

        [ProtoMember(3)]
        public ushort currentPlayerId = 0;

        public Dictionary<NitroxId, EquippedItemData> ModulesItemsById = new Dictionary<NitroxId, EquippedItemData>();

        private Dictionary<string, PersistedPlayerData> playersByPlayerName = new Dictionary<string, PersistedPlayerData>();
        
        public void AddEquipment(string playerName, EquippedItemData equippedItem)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerData = playersByPlayerName[playerName];
                playerData.EquippedItemsById.Add(equippedItem.ItemId, equippedItem);
            }
        }

        public ushort GetPlayerId(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);

                return playerPersistedData.PlayerId;
            }
        }

        public Vector3 GetPlayerSpawn(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);

                return playerPersistedData.PlayerSpawnData;
            }
        }

        public bool hasSeenPlayerBefore(string playerName)
        {
            lock (playersByPlayerName)
            {
                return playersByPlayerName.ContainsKey(playerName);
            }
        }

        public Perms GetPermissions(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);

                return playerPersistedData.Permissions;
            }
        }

        public void UpdatePlayerSpawn(string playerName, Vector3 position)
        {
            lock (playersByPlayerName)
            {
                playersByPlayerName[playerName].PlayerSpawnData = position;
            }
        }

        public bool SetPermissions(string playerName, Perms permissions)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData player;

                if (playersByPlayerName.TryGetValue(playerName, out player)) {
                    player.Permissions = permissions;
                    return true;
                }
            }

            return false;
        }
        
        public void UpdatePlayerSubRootId(string playerName, NitroxId subrootId)
        {
            lock (playersByPlayerName)
            {
                playersByPlayerName[playerName].SubRootId = subrootId;
            }
        }

        public void SetPlayerStats(string playerName, PlayerStats statsData)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);

                playersByPlayerName[playerName].CurrentStats = new PlayerStatsData(statsData.Oxygen, statsData.MaxOxygen, statsData.Health, statsData.Food, statsData.Water);
            }
        }
        public PlayerStatsData GetPlayerStats(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);

                return playersByPlayerName[playerName].CurrentStats;
            }
        }
                
        public void RemoveEquipment(string playerName, NitroxId id)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerData = playersByPlayerName[playerName];
                playerData.EquippedItemsById.Remove(id);
            }
        }

        public List<EquippedItemData> GetEquippedItemsForInitialSync(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);
                List<EquippedItemData> ItemData = new List<EquippedItemData>(playerPersistedData.EquippedItemsById.Values);
                ItemData.AddRange((new List<EquippedItemData>(ModulesItemsById.Values)));
                return ItemData;
            }
        }

        public Vector3 GetPosition(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);
                return playerPersistedData.PlayerSpawnData;
            }
        }

        public Optional<NitroxId> GetSubRootId(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);
                return Optional<NitroxId>.OfNullable(playerPersistedData.SubRootId);
            }
        }

        public NitroxId GetNitroxId(string playerName)
        {
            lock (playersByPlayerName)
            {
                PersistedPlayerData playerPersistedData = GetOrCreatePersistedPlayerData(playerName);
                return playerPersistedData.PlayerNitroxId;
            }
        }


        public void SetPlayerNitroxID(string playerName, NitroxId id)
        {
            lock (playersByPlayerName)
            {
                playersByPlayerName[playerName].PlayerNitroxId = id;
            }
        }

        // Must be called when playersByPlayerName is locked.
        private PersistedPlayerData GetOrCreatePersistedPlayerData(string playerName)
        {
            PersistedPlayerData playerPersistedData = null;

            if (!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
            {
                playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName, ++currentPlayerId, new NitroxId());
            }

            return playerPersistedData;
        }

        public void AddModule(EquippedItemData equippedmodule)
        {
            lock (ModulesItemsById)
            {
                ModulesItemsById.Add(equippedmodule.ItemId, equippedmodule);
            }
        }
        public void RemoveModule(NitroxId id)
        {
            lock (ModulesItemsById)
            {
                ModulesItemsById.Remove(id);
            }
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            playersByPlayerName = serializablePlayersByPlayerName;
            ModulesItemsById = serializableModules;
        }

        [ProtoContract]
        public class PersistedPlayerData
        {
            [ProtoMember(1)]
            public string PlayerName { get; set; }

            [ProtoMember(2)]
            public Dictionary<NitroxId, EquippedItemData> EquippedItemsById { get; set; } = new Dictionary<NitroxId, EquippedItemData>();

            [ProtoMember(3)]
            public ushort PlayerId { get; set; }

            [ProtoMember(4)]
            public Vector3 PlayerSpawnData { get; set; }

            [ProtoMember(5)]
            public PlayerStatsData CurrentStats { get; set; }

            [ProtoMember(6)]
            public NitroxId SubRootId { get; set; }

            [ProtoMember(7)]
            public Perms Permissions { get; set; } = Perms.PLAYER;

            [ProtoMember(8)]
            public NitroxId PlayerNitroxId { get; set; }


            public PersistedPlayerData()
            {
                // Constructor for serialization purposes
            }

            public PersistedPlayerData(string playerName, ushort playerId, NitroxId nitroxId)
            {
                PlayerName = playerName;
                PlayerId = playerId;
                PlayerNitroxId = nitroxId;
            }
        }
    }
}
