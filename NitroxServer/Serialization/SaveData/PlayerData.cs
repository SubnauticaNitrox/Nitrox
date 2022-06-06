using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NitroxServer.GameLogic;

namespace NitroxServer.Serialization.SaveData;

[JsonObject(MemberSerialization.OptIn)]
public class PlayerData
{
    [JsonProperty]
    public List<PersistedPlayerData> Players = new();

    public List<Player> GetPlayers()
    {
        return Players.Select(playerData => playerData.ToPlayer()).ToList();
    }

    public static PlayerData From(IEnumerable<Player> players)
    {
        return new PlayerData
        {
            Players = players.Select(PersistedPlayerData.FromPlayer).ToList()
        };
    }
}
