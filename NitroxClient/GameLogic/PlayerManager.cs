using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.MonoBehaviours;
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
            RemotePlayer remotePlayer = new RemotePlayer(remotePlayerBody, playerContext, playerModelManager);

            DiscordController.Main.UpdateDRPDiving(GetTotalPlayerCount());

            playersById.Add(remotePlayer.PlayerId, remotePlayer);

            return remotePlayer;
        }

        public void RemovePlayer(ushort playerId)
        {
            Optional<RemotePlayer> opPlayer = Find(playerId);
            if (opPlayer.IsPresent())
            {
                opPlayer.Get().Destroy();
                playersById.Remove(playerId);
                DiscordController.Main.UpdateDRPDiving(GetTotalPlayerCount());
            }
        }

        private GameObject CloneLocalPlayerBodyPrototype()
        {
            return Object.Instantiate(localPlayer.BodyPrototype);
        }

        public int GetTotalPlayerCount()
        {
            return playersById.Count + 1; //Multiplayer-player(s) + you
        }
    }
}
