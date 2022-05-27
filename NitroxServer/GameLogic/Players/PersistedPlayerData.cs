using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
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
        public List<NitroxTechType> UsedItems { get; set; } = new List<NitroxTechType>();

        [JsonProperty, ProtoMember(3)]
        public List<string> QuickSlotsBinding { get; set; } = new List<string>();

        [JsonProperty, ProtoMember(4)]
        public List<EquippedItemData> EquippedItems { get; set; } = new List<EquippedItemData>();

        [JsonProperty, ProtoMember(5)]
        public List<EquippedItemData> Modules { get; set; } = new List<EquippedItemData>();

        [JsonProperty, ProtoMember(6)]
        public ushort Id { get; set; }

        [JsonProperty, ProtoMember(7)]
        public NitroxVector3 SpawnPosition { get; set; }

        [JsonProperty, ProtoMember(8)]
        public PlayerStatsData CurrentStats { get; set; }

        [JsonProperty, ProtoMember(9)]
        public NitroxId SubRootId { get; set; }

        [JsonProperty, ProtoMember(10)]
        public Perms Permissions { get; set; }

        [JsonProperty, ProtoMember(11)]
        public NitroxId NitroxId { get; set; }

        [JsonProperty, ProtoMember(12)]
        public bool IsPermaDeath { get; set; }

        [JsonProperty, ProtoMember(13)]
        public HashSet<string> CompletedGoals { get; set; } = new HashSet<string>();

        [JsonProperty, ProtoMember(14)]
        public Dictionary<string, PingInstancePreference> PingInstancePreferences { get; set; } = new();

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
                              UsedItems,
                              QuickSlotsBinding,
                              EquippedItems,
                              Modules,
                              CompletedGoals,
                              PingInstancePreferences);
        }

        public static PersistedPlayerData FromPlayer(Player player)
        {
            return new PersistedPlayerData
            {
                Name = player.Name,
                UsedItems = player.UsedItems?.ToList(),
                QuickSlotsBinding = player.QuickSlotsBinding?.ToList(),
                EquippedItems = player.GetEquipment(),
                Modules = player.GetModules(),
                Id = player.Id,
                SpawnPosition = player.Position,
                CurrentStats = player.Stats,
                SubRootId = player.SubRootId.OrNull(),
                Permissions = player.Permissions,
                NitroxId = player.GameObjectId,
                IsPermaDeath = player.IsPermaDeath,
                CompletedGoals = new(player.CompletedGoals),
                PingInstancePreferences = player.PingInstancePreferences
            };
        }
    }
}
