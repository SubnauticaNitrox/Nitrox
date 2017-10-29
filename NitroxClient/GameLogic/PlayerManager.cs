using System.Collections.Generic;
using NitroxModel.DataStructures.Util;

namespace NitroxClient.GameLogic
{
    public class PlayerManager
    {
        private readonly Dictionary<string, RemotePlayer> playersById = new Dictionary<string, RemotePlayer>();

        public Optional<RemotePlayer> Find(string playerId)
        {
            RemotePlayer player;

            if (playersById.TryGetValue(playerId, out player))
            {
                return Optional<RemotePlayer>.Of(player);
            }

            return Optional<RemotePlayer>.Empty();
        }

        public RemotePlayer FindOrCreate(string playerId)
        {
            RemotePlayer player;

            if (!playersById.TryGetValue(playerId, out player))
            {
                player = playersById[playerId] = new RemotePlayer(playerId);
            }

            return player;
        }

        public void RemovePlayer(string playerId)
        {
            Optional<RemotePlayer> opPlayer = Find(playerId);
            if (opPlayer.IsPresent())
            {
                opPlayer.Get().Destroy();
                playersById.Remove(playerId);
            }
        }

        public void RemoveAllPlayers()
        {
            foreach (string playerId in playersById.Keys)
            {
                RemovePlayer(playerId);
            }
        }
    }
}
