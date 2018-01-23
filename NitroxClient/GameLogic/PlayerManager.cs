using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
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
                UpdateDiscordRichPresence();
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

        public int GetPlayerCount()
        {
            if (playersById == null)
            {
                return 0;
            }
            return playersById.Count;
        }

        private void UpdateDiscordRichPresence()
        {
            Multiplayer.DiscordRP.Presence.partySize = 1 + GetPlayerCount();
            Multiplayer.DiscordRP.UpdatePresence();
        }
    }
}
