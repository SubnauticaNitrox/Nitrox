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
        
        public PersistedPlayerData GetPersistedData(string playerName)
        {
            PersistedPlayerData playerPersistedData = null;

            lock (playersByPlayerName)
            {
                if(!playersByPlayerName.TryGetValue(playerName, out playerPersistedData))
                {
                    playerPersistedData = playersByPlayerName[playerName] = new PersistedPlayerData(playerName);
                }
            }

            return playerPersistedData;
        }        

        [ProtoContract]
        public class PersistedPlayerData
        {
            [ProtoMember(1)]
            public string PlayerName { get; set; }

            [ProtoMember(2)]
            public string InventoryGuid { get; set; }

            public PersistedPlayerData()
            {
                // Constructor for serialization purposes
            }

            public PersistedPlayerData(string playerName)
            {
                this.PlayerName = playerName;
                this.InventoryGuid = Guid.NewGuid().ToString();
            }
        }

    }
}
