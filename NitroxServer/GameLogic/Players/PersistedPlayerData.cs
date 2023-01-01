using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Players;

[DataContract]
public class PersistedPlayerData
{
    [DataMember(Order = 1)]
    public string Name { get; set; }

    [DataMember(Order = 2)]
    public List<NitroxTechType> UsedItems { get; set; } = new List<NitroxTechType>();

    [DataMember(Order = 3)]
    public List<string> QuickSlotsBinding { get; set; } = new List<string>();

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
    public NitroxId SubRootId { get; set; }

    [DataMember(Order = 11)]
    public Perms Permissions { get; set; }

    [DataMember(Order = 12)]
    public NitroxId NitroxId { get; set; }

    [DataMember(Order = 13)]
    public bool IsPermaDeath { get; set; }

    [DataMember(Order = 14)]
    public HashSet<string> CompletedGoals { get; set; } = new HashSet<string>();

    [DataMember(Order = 15)]
    public Dictionary<string, PingInstancePreference> PingInstancePreferences { get; set; } = new();

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
            SpawnRotation = player.Rotation,
            CurrentStats = player.Stats,
            SubRootId = player.SubRootId.OrNull(),
            Permissions = player.Permissions,
            NitroxId = player.GameObjectId,
            IsPermaDeath = player.IsPermaDeath,
            CompletedGoals = new(player.CompletedGoals),
            PingInstancePreferences = new(player.PingInstancePreferences)
        };
    }
}
