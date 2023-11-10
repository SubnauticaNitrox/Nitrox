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

namespace NitroxClient.GameLogic;

public class PlayerManager
{
    private readonly PlayerModelManager playerModelManager;
    private readonly PlayerVitalsManager playerVitalsManager;
    private readonly FMODWhitelist fmodWhitelist;
    private readonly Dictionary<ushort, RemotePlayer> playersById = new();

    public OnCreate onCreate;
    public OnRemove onRemove;

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

    public Optional<RemotePlayer> Find(NitroxId playerNitroxId)
    {
        RemotePlayer remotePlayer = playersById.Select(idToPlayer => idToPlayer.Value)
                                               .FirstOrDefault(player => player.PlayerContext.PlayerNitroxId == playerNitroxId);

        return Optional.OfNullable(remotePlayer);
    }

    internal IEnumerable<RemotePlayer> GetAll()
    {
        return playersById.Values;
    }

    public RemotePlayer Create(PlayerContext playerContext)
    {
        Validate.NotNull(playerContext);
        Validate.IsFalse(playersById.ContainsKey(playerContext.PlayerId));

        RemotePlayer remotePlayer = new(playerContext, playerModelManager, playerVitalsManager, fmodWhitelist);

        playersById.Add(remotePlayer.PlayerId, remotePlayer);
        onCreate(remotePlayer.PlayerId.ToString(), remotePlayer);

        DiscordClient.UpdatePartySize(GetTotalPlayerCount());

        return remotePlayer;
    }

    public void RemovePlayer(ushort playerId)
    {
        Optional<RemotePlayer> opPlayer = Find(playerId);
        if (opPlayer.HasValue)
        {
            opPlayer.Value.Destroy();
            playersById.Remove(playerId);
            onRemove(playerId.ToString(), opPlayer.Value);
            DiscordClient.UpdatePartySize(GetTotalPlayerCount());
        }
    }

    public int GetTotalPlayerCount()
    {
        return playersById.Count + 1; //Multiplayer-player(s) + you
    }

    public delegate void OnCreate(string playerId, RemotePlayer remotePlayer);
    public delegate void OnRemove(string playerId, RemotePlayer remotePlayer);
}
