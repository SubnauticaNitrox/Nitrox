using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class PlayerManager
    {
        private readonly ILocalNitroxPlayer localPlayer;
        private readonly Dictionary<ushort, RemotePlayer> playersById = new Dictionary<ushort, RemotePlayer>();

        public PlayerManager(ILocalNitroxPlayer localPlayer)
        {
            this.localPlayer = localPlayer;
        }

        public Optional<RemotePlayer> Find(ushort playerId)
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

        public RemotePlayer Create(PlayerContext playerContext)
        {
            Validate.NotNull(playerContext);

            if (playersById.ContainsKey(playerContext.PlayerId))
            {
                throw new Exception("The playerId has already been used.");
            }

            GameObject remotePlayerBody = CloneLocalPlayerBodyPrototype();
            RemotePlayer player = new RemotePlayer(remotePlayerBody, playerContext);

            PlayerModelDirector playerModelDirector = new PlayerModelDirector(player);
            playerModelDirector
                .AddPing()
                .AddDiveSuit();

            playerModelDirector.Construct();

            playersById.Add(player.PlayerId, player);
            UpdateDiscordRichPresence();
            return player;
        }

        public void RemovePlayer(ushort playerId)
        {
            Optional<RemotePlayer> opPlayer = Find(playerId);
            if (opPlayer.IsPresent())
            {
                opPlayer.Get().Destroy();
                playersById.Remove(playerId);
            }
        }

        private GameObject CloneLocalPlayerBodyPrototype()
        {
            return Object.Instantiate(localPlayer.BodyPrototype);
        }

        private void UpdateDiscordRichPresence()
        {
            Multiplayer.DiscordRP.Presence.partySize = 1 + GetPlayerCount();
            Multiplayer.DiscordRP.UpdatePresence();
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
