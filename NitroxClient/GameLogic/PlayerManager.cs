using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.MonoBehaviours.Discord;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class PlayerManager
{
    private readonly PlayerModelManager playerModelManager;
    private readonly PlayerVitalsManager playerVitalsManager;
    private readonly FMODWhitelist fmodWhitelist;
    private readonly Dictionary<ushort, RemotePlayer> playersById = new();

    public OnCreateDelegate OnCreate;
    public OnRemoveDelegate OnRemove;

    public PlayerManager(PlayerModelManager playerModelManager, PlayerVitalsManager playerVitalsManager, FMODWhitelist fmodWhitelist)
    {
        this.playerModelManager = playerModelManager;
        this.playerVitalsManager = playerVitalsManager;
        this.fmodWhitelist = fmodWhitelist;
    }

    public Optional<RemotePlayer> Find(ushort playerId)
    {
        playersById.TryGetValue(playerId, out RemotePlayer player);
        return Optional.OfNullable(player);
    }

    public bool TryFind(ushort playerId, out RemotePlayer remotePlayer) => playersById.TryGetValue(playerId, out remotePlayer);

    public Optional<RemotePlayer> Find(NitroxId playerNitroxId)
    {
        RemotePlayer remotePlayer = playersById.Select(idToPlayer => idToPlayer.Value)
                                               .FirstOrDefault(player => player.PlayerContext.PlayerNitroxId == playerNitroxId);

        return Optional.OfNullable(remotePlayer);
    }

    public IEnumerable<RemotePlayer> GetAll()
    {
        return playersById.Values;
    }

    public HashSet<GameObject> GetAllPlayerObjects()
    {
        HashSet<GameObject> remotePlayerObjects = GetAll().Select(player => player.Body).ToSet();
        remotePlayerObjects.Add(Player.mainObject);
        return remotePlayerObjects;
    }

    public RemotePlayer Create(PlayerContext playerContext)
    {
        Validate.NotNull(playerContext);
        Validate.IsFalse(playersById.ContainsKey(playerContext.PlayerId));

            RemotePlayer remotePlayer = new(playerContext, playerModelManager, playerVitalsManager, fmodWhitelist);
            
            playersById.Add(remotePlayer.PlayerId, remotePlayer);
            OnCreate(remotePlayer.PlayerId, remotePlayer);

        DiscordClient.UpdatePartySize(GetTotalPlayerCount());

        return remotePlayer;
    }

    public void RemovePlayer(ushort playerId)
    {
        if (playersById.TryGetValue(playerId, out RemotePlayer player))
        {
            player.Destroy();
            playersById.Remove(playerId);
            OnRemove(playerId, player);
            DiscordClient.UpdatePartySize(GetTotalPlayerCount());
        }
    }

    /// <returns>Remote players + You => X + 1</returns>
    public int GetTotalPlayerCount() => playersById.Count + 1;

    public delegate void OnCreateDelegate(ushort playerId, RemotePlayer remotePlayer);
    public delegate void OnRemoveDelegate(ushort playerId, RemotePlayer remotePlayer);
}
