using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.MonoBehaviours.Discord;
using Nitrox.Model.DataStructures;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class PlayerManager
{
    private readonly PlayerModelManager playerModelManager;
    private readonly PlayerVitalsManager playerVitalsManager;
    private readonly FMODWhitelist fmodWhitelist;
    private readonly Dictionary<SessionId, RemotePlayer> sessionsById = new();

    public OnCreateDelegate OnCreate;
    public OnRemoveDelegate OnRemove;

    public PlayerManager(PlayerModelManager playerModelManager, PlayerVitalsManager playerVitalsManager, FMODWhitelist fmodWhitelist)
    {
        this.playerModelManager = playerModelManager;
        this.playerVitalsManager = playerVitalsManager;
        this.fmodWhitelist = fmodWhitelist;
    }

    public Optional<RemotePlayer> Find(SessionId sessionId)
    {
        sessionsById.TryGetValue(sessionId, out RemotePlayer player);
        return Optional.OfNullable(player);
    }

    public bool TryFind(SessionId sessionId, out RemotePlayer remotePlayer) => sessionsById.TryGetValue(sessionId, out remotePlayer);

    public Optional<RemotePlayer> Find(NitroxId playerNitroxId)
    {
        RemotePlayer remotePlayer = sessionsById.Select(idToPlayer => idToPlayer.Value)
                                               .FirstOrDefault(player => player.PlayerContext.PlayerNitroxId == playerNitroxId);

        return Optional.OfNullable(remotePlayer);
    }

    public IEnumerable<RemotePlayer> GetAll()
    {
        return sessionsById.Values;
    }

    public HashSet<GameObject> GetAllPlayerObjects()
    {
        HashSet<GameObject> remotePlayerObjects = GetAll()
                                                  .Select(player => player.Body)
                                                  .Where(body => body != null)
                                                  .ToSet();
        if (Player.mainObject != null)
        {
            remotePlayerObjects.Add(Player.mainObject);
        }
        return remotePlayerObjects;
    }

    public RemotePlayer Create(PlayerContext playerContext)
    {
        Validate.NotNull(playerContext);
        Validate.IsFalse(sessionsById.ContainsKey(playerContext.SessionId));

        RemotePlayer remotePlayer = new(playerContext, playerModelManager, playerVitalsManager, fmodWhitelist);

        sessionsById.Add(remotePlayer.SessionId, remotePlayer);
        OnCreate(remotePlayer.SessionId, remotePlayer);

        DiscordClient.UpdatePartySize(GetTotalPlayerCount());

        return remotePlayer;
    }

    public void RemovePlayer(SessionId sessionId)
    {
        if (sessionsById.TryGetValue(sessionId, out RemotePlayer player))
        {
            player.Destroy();
            sessionsById.Remove(sessionId);
            OnRemove(sessionId, player);
            DiscordClient.UpdatePartySize(GetTotalPlayerCount());
        }
    }

    /// <returns>Remote players + You => X + 1</returns>
    public int GetTotalPlayerCount() => sessionsById.Count + 1;

    public delegate void OnCreateDelegate(SessionId sessionId, RemotePlayer remotePlayer);
    public delegate void OnRemoveDelegate(SessionId sessionId, RemotePlayer remotePlayer);
}
