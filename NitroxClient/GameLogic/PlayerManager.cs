using System;
using System.Collections.Generic;

namespace NitroxClient.GameLogic
{
    public class PlayerManager
    {
        private readonly Dictionary<string, RemotePlayer> playersById = new Dictionary<string, RemotePlayer>();

        public RemotePlayer this[string playerId]
        {
            get
            {
                return FindPlayer(playerId);
            }
            set
            {
                if (value == null)
                {
                    RemovePlayer(playerId);
                }
                else
                {
                    playersById[playerId] = value;
                }
            }
        }

        public RemotePlayer FindPlayer(string playerId, bool createPlayer = false)
        {
            RemotePlayer player;

            if (!playersById.TryGetValue(playerId, out player) && createPlayer)
            {
                player = playersById[playerId] = new RemotePlayer(playerId);
            }

            return player;
        }

        public void ForPlayer(string playerId, Action<RemotePlayer> action, bool createPlayer = false)
        {
            var player = FindPlayer(playerId, createPlayer);

            if (player != null)
            {
                action(player);
            }
        }

        public void RemovePlayer(string playerId)
        {
            RemotePlayer remotePlayer = playersById[playerId];
            remotePlayer.Destroy();
            playersById.Remove(playerId);
        }

        public void RemoveAllPlayers()
        {
            foreach (String playerId in playersById.Keys)
            {
                RemovePlayer(playerId);
            }
        }
    }
}
