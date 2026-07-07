using System.Collections.Generic;
using System.Linq;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Players;

internal sealed class PlayerData
{
    public List<PersistedPlayerData> Players = [];

    public List<Player> GetPlayers()
    {
        return Players.Select(playerData => playerData.ToPlayer()).ToList();
    }

    public static PlayerData From(IEnumerable<Player> players)
    {
        List<PersistedPlayerData> persistedPlayers = players.Select(PersistedPlayerData.FromPlayer).ToList();

        return new PlayerData { Players = persistedPlayers };
    }
}
