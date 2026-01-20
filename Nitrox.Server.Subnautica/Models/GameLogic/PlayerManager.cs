using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

// TODO: This manager should only handle player data. Move connection related state to other managers.
internal sealed partial class PlayerManager(SessionManager sessionManager, IOptions<SubnauticaServerOptions> options, HibernateService hibernateService, ILogger<PlayerManager> logger)
{
    // https://regex101.com/r/eTWiEs/2/
    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,25}$", RegexOptions.NonBacktracking)]
    private static partial Regex PlayerNameRegex();

    private readonly ThreadSafeDictionary<string, Player> allPlayersByName = [];
    private readonly ThreadSafeDictionary<SessionId, Player> connectedPlayersBySessionId = [];
    private readonly ThreadSafeDictionary<INitroxConnection, ConnectionAssets> assetsByConnection = [];
    private readonly ThreadSafeDictionary<string, PlayerContext> reservations = [];
    private readonly ThreadSafeSet<string> reservedPlayerNames = new("Player"); // "Player" is often used to identify the local player and should not be used by any user

    private readonly SessionManager sessionManager = sessionManager;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly HibernateService hibernateService = hibernateService;
    private readonly ILogger<PlayerManager> logger = logger;
    private PeerId currentPlayerId;

    /// <summary>All players that have joined since the server started, even if they disconnected</summary>
    public IEnumerable<Player> GetAllPlayers() => allPlayersByName.Values;

    public IEnumerable<Player> ConnectedPlayers()
    {
        return assetsByConnection.Values
                                 .Where(assetPackage => assetPackage.Player != null)
                                 .Select(assetPackage => assetPackage.Player);
    }

    public List<Player> GetConnectedPlayers() => ConnectedPlayers().ToList();

    public List<Player> GetConnectedPlayersExcept(Player excludePlayer)
    {
        return ConnectedPlayers().Where(player => player != excludePlayer).ToList();
    }

    public Player? GetPlayer(INitroxConnection connection)
    {
        return assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage) ? assetPackage.Player : null;
    }

    public PlayerContext? GetPlayerContext(string reservationKey)
    {
        return reservations.TryGetValue(reservationKey, out PlayerContext playerContext) ? playerContext : null;
    }

    public void AddSavedPlayer(Player player)
    {
        allPlayersByName.Add(player.Name, player);
        currentPlayerId = allPlayersByName.Values.Max(x => x.Id);
    }

    public MultiplayerSessionReservation ReservePlayerContext(
        INitroxConnection connection,
        PlayerSettings playerSettings,
        AuthenticationContext authenticationContext,
        string correlationId)
    {
        if (Math.Min(reservedPlayerNames.Count - 1, 0) >= options.Value.MaxConnections)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (!string.IsNullOrEmpty(options.Value.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != options.Value.ServerPassword))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }


        if (!PlayerNameRegex().IsMatch(authenticationContext.Username))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        string playerName = authenticationContext.Username;

        allPlayersByName.TryGetValue(playerName, out Player? player);
        if (player?.IsPermaDeath == true && options.Value.IsHardcore())
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.HARDCORE_PLAYER_DEAD;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (reservedPlayerNames.Contains(playerName))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
        if (assetPackage == null)
        {
            assetPackage = new ConnectionAssets();
            assetsByConnection.Add(connection, assetPackage);
            reservedPlayerNames.Add(playerName);
        }

        bool hasSeenPlayerBefore = player != null;
        NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();
        SubnauticaGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : options.Value.GameMode;
        IntroCinematicMode introCinematicMode = hasSeenPlayerBefore ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;
        PlayerAnimation animation = new(AnimChangeType.UNDERWATER, AnimChangeState.ON);

        SessionManager.Session session = sessionManager.GetOrCreateSession(connection.Endpoint);

        // TODO: At some point, store the muted state of a player
        PlayerContext playerContext = new(playerName, session.Id, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode, animation);
        string reservationKey = Guid.NewGuid().ToString();

        reservations.Add(reservationKey, playerContext);
        assetPackage.ReservationKey = reservationKey;

        return new MultiplayerSessionReservation(correlationId, session.Id, reservationKey);
    }

    public Player CreatePlayerData(INitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
    {
        PlayerContext playerContext = reservations[reservationKey];
        Validate.NotNull(playerContext);
        ConnectionAssets assetPackage = assetsByConnection[connection];
        Validate.NotNull(assetPackage);

        wasBrandNewPlayer = playerContext.WasBrandNewPlayer;

        if (!allPlayersByName.TryGetValue(playerContext.PlayerName, out Player player))
        {
            player = new Player(++currentPlayerId,
                                playerContext.PlayerName,
                                false,
                                playerContext,
                                connection,
                                NitroxVector3.Zero,
                                NitroxQuaternion.Identity,
                                playerContext.PlayerNitroxId,
                                Optional.Empty,
                                options.Value.DefaultPlayerPerm,
                                new(options.Value.DefaultOxygenValue, options.Value.DefaultMaxOxygenValue, options.Value.DefaultHealthValue, options.Value.DefaultHungerValue, options.Value.DefaultThirstValue, options.Value.DefaultInfectionValue),
                                options.Value.GameMode,
                                [],
                                [],
                                new Dictionary<string, NitroxId>(),
                                new Dictionary<string, float>(),
                                new Dictionary<string, PingInstancePreference>(),
                                [],
                                false,
                                true
            );
            allPlayersByName[playerContext.PlayerName] = player;
        }

        connectedPlayersBySessionId.Add(playerContext.SessionId, player);

        // TODO: make a ConnectedPlayer wrapper so this is not stateful
        player.PlayerContext = playerContext;
        player.Connection = connection;

        // reconnecting players need to have their cell visibility refreshed
        player.ClearVisibleCells();

        assetPackage.Player = player;
        assetPackage.ReservationKey = null;
        reservations.Remove(reservationKey);

        return player;
    }

    public int PlayerCount => connectedPlayersBySessionId.Count;

    public void RemovePlayer(SessionId sessionId)
    {
        if (!connectedPlayersBySessionId.TryGetValue(sessionId, out Player player))
        {
            return;
        }
        if (!assetsByConnection.TryGetValue(player.Connection, out ConnectionAssets assetPackage))
        {
            return;
        }

        if (assetPackage.ReservationKey != null)
        {
            PlayerContext playerContext = reservations[assetPackage.ReservationKey];
            reservedPlayerNames.Remove(playerContext.PlayerName);
            reservations.Remove(assetPackage.ReservationKey);
        }

        if (assetPackage.Player != null)
        {
            reservedPlayerNames.Remove(player.Name);
            connectedPlayersBySessionId.Remove(player.SessionId);
            logger.ZLogInformation($"{player.Name} left the game");
        }

        assetsByConnection.Remove(player.Connection);

        if (!ConnectedPlayers().Any())
        {
            // TODO: Make this function async
            _ = hibernateService.SleepAsync().ContinueWithHandleError();
        }
    }

    public bool SetPlayerProperty<T>(SessionId playerId, T value, Action<Player, T> action)
    {
        if (!TryGetPlayerBySessionId(playerId, out Player? player))
        {
            return false;
        }

        action(player, value);
        return true;
    }

    public bool TryGetPlayerByName(string playerName, [NotNullWhen(true)] out Player? foundPlayer)
    {
        foundPlayer = null;
        foreach (Player player in ConnectedPlayers())
        {
            if (player.Name == playerName)
            {
                foundPlayer = player;
                return true;
            }
        }

        return false;
    }

    public bool TryGetPlayerBySessionId(SessionId sessionId, [NotNullWhen(true)] out Player? player)
    {
        return connectedPlayersBySessionId.TryGetValue(sessionId, out player);
    }
}
