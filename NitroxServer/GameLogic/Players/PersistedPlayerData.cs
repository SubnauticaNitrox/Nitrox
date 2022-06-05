using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Players;

[JsonObject(MemberSerialization.OptIn)]
public class PersistedPlayerData
{
    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public List<NitroxTechType> UsedItems { get; set; } = new();

    [JsonProperty]
    public List<string> QuickSlotsBinding { get; set; } = new();

    [JsonProperty]
    public List<EquippedItemData> EquippedItems { get; set; } = new();

    [JsonProperty]
    public List<EquippedItemData> Modules { get; set; } = new();

    [JsonProperty]
    public ushort Id { get; set; }

    [JsonProperty]
    public NitroxVector3 SpawnPosition { get; set; }

    [JsonProperty]
    public PlayerStatsData CurrentStats { get; set; }

    [JsonProperty]
    public NitroxId SubRootId { get; set; }

    [JsonProperty]
    public Perms Permissions { get; set; }

    [JsonProperty]
    public NitroxId NitroxId { get; set; }

    [JsonProperty]
    public bool IsPermaDeath { get; set; }

    [JsonProperty]
    public HashSet<string> CompletedGoals { get; set; } = new();

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
                          CompletedGoals);
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
            CompletedGoals = new HashSet<string>(player.CompletedGoals)
        };
    }
}
