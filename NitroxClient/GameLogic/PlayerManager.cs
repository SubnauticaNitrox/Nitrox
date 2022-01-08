using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class PlayerManager
    {
        private readonly IPacketSender packetSender;
        private readonly ILocalNitroxPlayer localPlayer;
        private readonly PlayerModelManager playerModelManager;
        private readonly FMODSystem fmodSystem;
        private readonly Dictionary<ushort, RemotePlayer> playersById = new Dictionary<ushort, RemotePlayer>();

        public PlayerManager(IPacketSender packetSender, ILocalNitroxPlayer localPlayer, PlayerModelManager playerModelManager, FMODSystem fmodSystem)
        {
            this.packetSender = packetSender;
            this.localPlayer = localPlayer;
            this.playerModelManager = playerModelManager;
            this.fmodSystem = fmodSystem;
        }

        public Optional<RemotePlayer> Find(ushort playerId)
        {
            playersById.TryGetValue(playerId, out RemotePlayer player);
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

        public RemotePlayer Create(PlayerContext playerContext, Optional<NitroxId> subRootId, List<TechType> equippedTechTypes, List<Pickupable> inventoryItems)
        {
            Validate.NotNull(playerContext);

            if (playersById.ContainsKey(playerContext.PlayerId))
            {
                throw new Exception("The playerId has already been used.");
            }

            GameObject remotePlayerBody = CloneLocalPlayerBodyPrototype();
            RemotePlayer remotePlayer;
            using (packetSender.Suppress<ItemContainerAdd>())
            {
                remotePlayer = new RemotePlayer(remotePlayerBody, playerContext, equippedTechTypes, inventoryItems, playerModelManager, fmodSystem);
            }

            if (subRootId.HasValue)
            {
                Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Value);
                if (sub.HasValue && sub.Value.TryGetComponent(out SubRoot subRoot))
                {
                    Log.Debug($"Found sub root for {playerContext.PlayerName}. Will add him and update animation.");
                    remotePlayer.SetSubRoot(subRoot);
                }
                else if (sub.HasValue && sub.Value.TryGetComponent(out EscapePod escapePod))
                {
                    Log.Debug($"Found EscapePod for {playerContext.PlayerName}.");
                    remotePlayer.SetEscapePod(escapePod);
                }
                else
                {
                    Log.Error($"Found neither SubRoot component nor EscapePod on {subRootId.Value} for {playerContext.PlayerName}.");
                }
            }

            playersById.Add(remotePlayer.PlayerId, remotePlayer);

            DiscordRPController.Main.UpdatePlayerCount(GetTotalPlayerCount());

            return remotePlayer;
        }

        public void RemovePlayer(ushort playerId)
        {
            Optional<RemotePlayer> opPlayer = Find(playerId);
            if (opPlayer.HasValue)
            {
                using (packetSender.Suppress<ItemContainerRemove>())
                {
                    opPlayer.Value.Destroy();
                }
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
