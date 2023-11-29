using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Server;

namespace NitroxServer.GameLogic.Players;

[DataContract]
public class PersistedPlayerData
{
    [DataMember(Order = 1)]
    public string Name { get; set; }

    [DataMember(Order = 2)]
    public List<NitroxTechType> UsedItems { get; set; } = new List<NitroxTechType>();

    [DataMember(Order = 3)]
    public Optional<NitroxId>[] QuickSlotsBindingIds { get; set; } = Array.Empty<Optional<NitroxId>>();

    [DataMember(Order = 4)]
    public List<EquippedItemData> EquippedItems { get; set; } = new List<EquippedItemData>();

    [DataMember(Order = 5)]
    public List<EquippedItemData> Modules { get; set; } = new List<EquippedItemData>();

    [DataMember(Order = 6)]
    public ushort Id { get; set; }

    [DataMember(Order = 7)]
    public NitroxVector3 SpawnPosition { get; set; }

    [DataMember(Order = 8)]
    public NitroxQuaternion SpawnRotation { get; set; }

    [DataMember(Order = 9)]
    public PlayerStatsData CurrentStats { get; set; }

    [DataMember(Order = 10)]
    public NitroxGameMode GameMode { get; set; }

    [DataMember(Order = 11)]
    public NitroxId SubRootId { get; set; }

    [DataMember(Order = 12)]
    public Perms Permissions { get; set; }

    [DataMember(Order = 13)]
    public NitroxId NitroxId { get; set; }

    [DataMember(Order = 14)]
    public bool IsPermaDeath { get; set; }

    /// <summary>
    /// Those goals are unlocked individually (e.g. opening PDA, eating, picking up a fire extinguisher for the first time)
    /// </summary>
    [DataMember(Order = 15)]
    public Dictionary<string, float> PersonalCompletedGoalsWithTimestamp { get; set; } = new Dictionary<string, float>();

    [DataMember(Order = 16)]
    public SubnauticaPlayerPreferences PlayerPreferences { get; set; }

    public Player ToPlayer()
    {
        return new Player(Id,
                          Name,
                          IsPermaDeath,
                          null, //no connection/context as this player is not connected.
                          null,
                          SpawnPosition,
                          SpawnRotation,
                          NitroxId,
                          Optional.OfNullable(SubRootId),
                          Permissions,
                          CurrentStats,
                          GameMode,
                          UsedItems,
                          QuickSlotsBindingIds,
                          EquippedItems,
                          Modules,
                          PersonalCompletedGoalsWithTimestamp,
                          PlayerPreferences.PingPreferences,
                          PlayerPreferences.PinnedTechTypes);
    }

    public static PersistedPlayerData FromPlayer(Player player)
    {
        return new PersistedPlayerData
        {
            Name = player.Name,
            UsedItems = player.UsedItems?.ToList(),
            QuickSlotsBindingIds = player.QuickSlotsBindingIds,
            EquippedItems = player.GetEquipment(),
            Modules = player.GetModules(),
            Id = player.Id,
            SpawnPosition = player.Position,
            SpawnRotation = player.Rotation,
            CurrentStats = player.Stats,
            GameMode = player.GameMode,
            SubRootId = player.SubRootId.OrNull(),
            Permissions = player.Permissions,
            NitroxId = player.GameObjectId,
            IsPermaDeath = player.IsPermaDeath,
            PersonalCompletedGoalsWithTimestamp = new(player.PersonalCompletedGoalsWithTimestamp),
            PlayerPreferences = new(player.PingInstancePreferences.ToDictionary(m => m.Key, m => m.Value), player.PinnedRecipePreferences.ToList())
        };
    }
}
