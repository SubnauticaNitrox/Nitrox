using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;

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

        internal Optional<RemotePlayer> FindByName(string playerName)
        {
            foreach (RemotePlayer player in playersById.Values)
            {
                if (player.PlayerName == playerName)
                {
                    return Optional<RemotePlayer>.Of(player);
                }
            }
            return Optional<RemotePlayer>.Empty();
        }

        public void Create(string playerId, string playerName, PlayerSettings playerSettings)
        {
            if (playersById.ContainsKey(playerId))
            {
                throw new Exception("The playerId has already been used.");
            }

            RemotePlayer player = new RemotePlayer(playerId, playerName, playerSettings);

            PlayerModelDirector playerModelDirector = new PlayerModelDirector(player);
            playerModelDirector
                .StagePlayer()
                .WithPing()
                .WithRegularDiveSuit();

            playerModelDirector.Construct();

            playersById.Add(playerId, player);
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
                Optional<RemotePlayer> opPlayer = Find(playerId);
                if (opPlayer.IsPresent())
                {
                    opPlayer.Get().Destroy();
                }
            }

            playersById.Clear();
        }
    }
}
