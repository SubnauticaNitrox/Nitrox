using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours.DiscordRP;
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
        private readonly PlayerModelManager playerModelManager;
        private readonly Dictionary<ushort, RemotePlayer> playersById = new Dictionary<ushort, RemotePlayer>();

        public PlayerManager(ILocalNitroxPlayer localPlayer, PlayerModelManager playerModelManager)
        {
            this.localPlayer = localPlayer;
            this.playerModelManager = playerModelManager;
        }

        public Optional<RemotePlayer> Find(ushort playerId)
        {
            RemotePlayer player;
            playersById.TryGetValue(playerId, out player);
            return Optional.OfNullable(player);
        }

        internal Optional<RemotePlayer> FindByName(string playerName)
        {
            foreach (RemotePlayer player in playersById.Values)
            {
                if (player.PlayerName == playerName)
                {
                    return Optional.Of(player);
                }
            }

            return Optional.Empty;
        }

        internal IEnumerable<RemotePlayer> GetAll()
        {
            return playersById.Values;
        }

        public RemotePlayer Create(PlayerContext playerContext, List<TechType> equippedTechTypes)
        {
            Validate.NotNull(playerContext);

            if (playersById.ContainsKey(playerContext.PlayerId))
            {
                throw new Exception("The playerId has already been used.");
            }

            GameObject remotePlayerBody = CloneLocalPlayerBodyPrototype();
            RemotePlayer remotePlayer = new RemotePlayer(remotePlayerBody, playerContext, equippedTechTypes, playerModelManager);

            DiscordRPController.Main.UpdatePlayerCount(GetTotalPlayerCount());

            playersById.Add(remotePlayer.PlayerId, remotePlayer);

            return remotePlayer;
        }

        public void RemovePlayer(ushort playerId)
        {
            Optional<RemotePlayer> opPlayer = Find(playerId);
            if (opPlayer.HasValue)
            {
                opPlayer.Value.Destroy();
                playersById.Remove(playerId);
                DiscordRPController.Main.UpdatePlayerCount(GetTotalPlayerCount());
            }
        }

        private GameObject CloneLocalPlayerBodyPrototype()
        {
            GameObject clone = Object.Instantiate(localPlayer.BodyPrototype);
            clone.SetActive(true);
            return clone;
        }

        public int GetTotalPlayerCount()
        {
            return playersById.Count + 1; //Multiplayer-player(s) + you
        }
    }
}
