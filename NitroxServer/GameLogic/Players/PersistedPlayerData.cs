using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PersistedPlayerData
    {
        [JsonProperty, ProtoMember(1)]
        public string Name { get; set; }

        [JsonProperty, ProtoMember(2)]
        public List<EquippedItemData> EquippedItems { get; set; } = new List<EquippedItemData>();

        [JsonProperty, ProtoMember(3)]
        public List<EquippedItemData> Modules { get; set; } = new List<EquippedItemData>();

        [JsonProperty, ProtoMember(4)]
        public ushort Id { get; set; }

        [JsonProperty, ProtoMember(5)]
        public NitroxVector3 SpawnPosition { get; set; }

        [JsonProperty, ProtoMember(6)]
        public PlayerStatsData CurrentStats { get; set; }

        [JsonProperty, ProtoMember(7)]
        public NitroxId SubRootId { get; set; }

        [JsonProperty, ProtoMember(8)]
        public Perms Permissions { get; set; } = Perms.PLAYER;

        [JsonProperty, ProtoMember(9)]
        public NitroxId NitroxId { get; set; }

        [JsonProperty, ProtoMember(10)]
        public bool IsPermaDeath { get; set; }

        public Player ToPlayer()
        {
            return new Player(Id,
                              Name,
                              IsPermaDeath,
                              null, //no connection/context as this player is not connected.
                              null,
                              SpawnPosition,
                              NitroxId,
                              Optional.OfNullable(SubRootId),
                              Permissions,
                              CurrentStats,
                              EquippedItems,
                              Modules);
        }

        public static PersistedPlayerData FromPlayer(Player player)
        {
            return new PersistedPlayerData
            {
                Name = player.Name,
                EquippedItems = player.GetEquipment(),
                Modules = player.GetModules(),
                Id = player.Id,
                SpawnPosition = player.Position,
                CurrentStats = player.Stats,
                SubRootId = player.SubRootId.OrElse(null),
                Permissions = player.Permissions,
                NitroxId = player.GameObjectId,
                IsPermaDeath = player.IsPermaDeath
            };
        }
    }
}
