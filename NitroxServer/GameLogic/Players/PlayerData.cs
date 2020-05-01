using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using System.Collections.Generic;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract]
    public class PlayerData
    {
        public const long VERSION = 2;

        [ProtoMember(1)]
        public List<PersistedPlayerData> Players = new List<PersistedPlayerData>();
        
        public List<Player> GetPlayers()
        {
            List<Player> boPlayers = new List<Player>();

            foreach (PersistedPlayerData playerData in Players)
            {
                Player player = new Player(playerData.Id,
                                           playerData.Name,
                                           playerData.IsPermaDeath,
                                           null, //no connection/context as this player is not connected.
                                           null, 
                                           playerData.SpawnPosition, 
                                           playerData.NitroxId, 
                                           NitroxModel.DataStructures.Util.Optional.OfNullable(playerData.SubRootId), 
                                           playerData.Permissions, 
                                           playerData.CurrentStats,
                                           playerData.EquippedItems, 
                                           playerData.Modules);
                
                boPlayers.Add(player);
            }

            return boPlayers;
        }

        public static PlayerData From(IEnumerable<Player> players)
        {
            List<PersistedPlayerData> persistedPlayers = new List<PersistedPlayerData>();

            foreach (Player player in players)
            {
                PersistedPlayerData persistedPlayer = new PersistedPlayerData();
                persistedPlayer.Name = player.Name;
                persistedPlayer.EquippedItems = player.GetEquipment();
                persistedPlayer.Modules = player.GetModules();
                persistedPlayer.Id = player.Id;
                persistedPlayer.SpawnPosition = player.Position;
                persistedPlayer.CurrentStats = player.Stats;
                persistedPlayer.SubRootId = player.SubRootId.OrElse(null);
                persistedPlayer.Permissions = player.Permissions;
                persistedPlayer.NitroxId = player.GameObjectId;
                persistedPlayer.IsPermaDeath = player.IsPermaDeath;

                persistedPlayers.Add(persistedPlayer);
            }

            PlayerData playerData = new PlayerData();
            playerData.Players = persistedPlayers;

            return playerData;
        }

        [ProtoContract]
        public class PersistedPlayerData
        {
            [ProtoMember(1)]
            public string Name { get; set; }

            [ProtoMember(2)]
            public List<EquippedItemData> EquippedItems { get; set; } = new List<EquippedItemData>();

            [ProtoMember(3)]
            public List<EquippedItemData> Modules { get; set; } = new List<EquippedItemData>();

            [ProtoMember(4)]
            public ushort Id { get; set; }

            [ProtoMember(5)]
            public Vector3 SpawnPosition { get; set; }

            [ProtoMember(6)]
            public PlayerStatsData CurrentStats { get; set; }

            [ProtoMember(7)]
            public NitroxId SubRootId { get; set; }

            [ProtoMember(8)]
            public Perms Permissions { get; set; } = Perms.PLAYER;

            [ProtoMember(9)]
            public NitroxId NitroxId { get; set; }
            [ProtoMember(10)]
            public bool IsPermaDeath { get; set; }


        }
    }
}
