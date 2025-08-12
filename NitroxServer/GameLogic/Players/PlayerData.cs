using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NitroxServer.GameLogic.Players
{
    [DataContract]
    public class PlayerData
    {
        [DataMember(Order = 1)]
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
}
