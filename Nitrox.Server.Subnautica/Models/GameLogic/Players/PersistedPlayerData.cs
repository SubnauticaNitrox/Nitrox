using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Players;

internal sealed class PersistedPlayerData
{
    public string Name { get; set; }

    public List<NitroxTechType> UsedItems { get; set; } = [];

    public Optional<NitroxId>[] QuickSlotsBindingIds { get; set; } = [];

    public Dictionary<string, NitroxId> EquippedItems { get; set; } = [];

    public PeerId Id { get; set; }

    public NitroxVector3 SpawnPosition { get; set; }

    public NitroxQuaternion SpawnRotation { get; set; }

    public PlayerStatsData CurrentStats { get; set; }

    public SubnauticaGameMode GameMode { get; set; }

    public NitroxId SubRootId { get; set; }

    public Perms Permissions { get; set; }

    public NitroxId NitroxId { get; set; }

    public bool IsPermaDeath { get; set; }

    /// <summary>
    /// Those goals are unlocked individually (e.g. opening PDA, eating, picking up a fire extinguisher for the first time)
    /// </summary>
    public Dictionary<string, float> PersonalCompletedGoalsWithTimestamp { get; set; } = [];

    public SubnauticaPlayerPreferences? PlayerPreferences { get; set; }

    public bool InPrecursor { get; set; }

    public bool DisplaySurfaceWater { get; set; }

    public bool HasUsedConsole { get; set; }

    public Player ToPlayer()
    {
        return new Player(Id,
                          0,
                          Name,
                          IsPermaDeath,
                          null, //no connection/context as this player is not connected.
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
                          PersonalCompletedGoalsWithTimestamp,
                          PlayerPreferences.PingPreferences,
                          PlayerPreferences.PinnedTechTypes,
                          InPrecursor,
                          DisplaySurfaceWater,
                          HasUsedConsole);
    }

    public static PersistedPlayerData FromPlayer(Player player)
    {
        return new PersistedPlayerData
        {
            Name = player.Name,
            UsedItems = player.UsedItems?.ToList(),
            QuickSlotsBindingIds = player.QuickSlotsBindingIds,
            EquippedItems = new(player.EquippedItems),
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
            PlayerPreferences = new(player.PingInstancePreferences.ToDictionary(m => m.Key, m => m.Value), player.PinnedRecipePreferences.ToList()),
            InPrecursor = player.InPrecursor,
            DisplaySurfaceWater = player.DisplaySurfaceWater,
            HasUsedConsole = player.HasUsedConsole
        };
    }
}
