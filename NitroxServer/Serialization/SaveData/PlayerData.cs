using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class PlayerData
{
    [JsonProperty]
    public List<PersistedPlayerData> Players = new();

    public List<GameLogic.Player> GetPlayers()
    {
        return Players.Select(playerData => playerData.ToPlayer()).ToList();
    }

    public static PlayerData From(IEnumerable<GameLogic.Player> players)
    {
        List<PersistedPlayerData> persistedPlayers = players.Select(PersistedPlayerData.FromPlayer).ToList();

        return new PlayerData { Players = persistedPlayers };
    }
}
