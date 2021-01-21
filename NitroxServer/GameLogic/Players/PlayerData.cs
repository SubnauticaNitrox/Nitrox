using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PlayerData
    {
        [JsonProperty, ProtoMember(1)]
        public List<PersistedPlayerData> Players = new List<PersistedPlayerData>();

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
}
