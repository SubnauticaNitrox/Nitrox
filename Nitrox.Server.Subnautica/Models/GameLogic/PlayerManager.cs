using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.DataStructures.Util;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Serialization;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.Communication;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

// TODO: These methods are a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
public partial class PlayerManager
{
    // https://regex101.com/r/eTWiEs/2/
    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,25}$", RegexOptions.NonBacktracking)]
    private static partial Regex PlayerNameRegex();

    private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
    private readonly ThreadSafeDictionary<ushort, Player> connectedPlayersById = [];
    private readonly ThreadSafeDictionary<INitroxConnection, ConnectionAssets> assetsByConnection = [];
    private readonly ThreadSafeDictionary<string, PlayerContext> reservations = [];
    private readonly ThreadSafeSet<string> reservedPlayerNames = new("Player"); // "Player" is often used to identify the local player and should not be used by any user

    private ThreadSafeQueue<KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>> JoinQueue { get; set; } = new();
    private bool PlayerCurrentlyJoining { get; set; }

    private Timer initialSyncTimer;

    private readonly SubnauticaServerConfig serverConfig;
    private ushort currentPlayerId;

    public PlayerManager(List<Player> players, SubnauticaServerConfig serverConfig)
    {
        allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
        currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);

        this.serverConfig = serverConfig;
    }

    public List<Player> GetConnectedPlayers()
    {
        return ConnectedPlayers().ToList();
    }

    public List<Player> GetConnectedPlayersExcept(Player excludePlayer)
    {
        return ConnectedPlayers().Where(player => player != excludePlayer).ToList();
    }

    public IEnumerable<Player> GetAllPlayers()
    {
        return allPlayersByName.Values;
    }

    public MultiplayerSessionReservation ReservePlayerContext(
        INitroxConnection connection,
        PlayerSettings playerSettings,
        AuthenticationContext authenticationContext,
        string correlationId)
    {
        if (Math.Min(reservedPlayerNames.Count - 1, 0) >= serverConfig.MaxConnections)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != serverConfig.ServerPassword))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (!PlayerNameRegex().IsMatch(authenticationContext.Username))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (PlayerCurrentlyJoining)
        {
            if (JoinQueue.Any(pair => ReferenceEquals(pair.Key, connection)))
            {
                // Don't enqueue the request if there is already another enqueued request by the same user
                return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.REJECTED);
            }

            JoinQueue.Enqueue(new KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>(
                                  connection,
                                  new MultiplayerSessionReservationRequest(correlationId, playerSettings, authenticationContext)));

            return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE);
        }

        string playerName = authenticationContext.Username;

        allPlayersByName.TryGetValue(playerName, out Player player);
        if (player?.IsPermaDeath == true && serverConfig.IsHardcore())
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
        ushort playerId = hasSeenPlayerBefore ? player.Id : ++currentPlayerId;
        NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();
        NitroxGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : serverConfig.GameMode;
        IntroCinematicMode introCinematicMode = hasSeenPlayerBefore ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;
        PlayerAnimation animation = new(AnimChangeType.UNDERWATER, AnimChangeState.ON);

        // TODO: At some point, store the muted state of a player
        PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode, animation);
        string reservationKey = Guid.NewGuid().ToString();

        reservations.Add(reservationKey, playerContext);
        assetPackage.ReservationKey = reservationKey;

        PlayerCurrentlyJoining = true;

        InitialSyncTimerData timerData = new(connection, authenticationContext, serverConfig.InitialSyncTimeout);
        initialSyncTimer = new Timer(InitialSyncTimerElapsed, timerData, 0, 200);


        return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
    }

    private void InitialSyncTimerElapsed(object state)
    {
        if (state is InitialSyncTimerData timerData && !timerData.Disposing)
        {
            allPlayersByName.TryGetValue(timerData.Context.Username, out Player player);

            if (timerData.Connection.State < NitroxConnectionState.Connected)
            {
                if (player == null) // player can cancel the joining process before this timer elapses
                {
                    Log.Error("Player was nulled while joining");
                    PlayerDisconnected(timerData.Connection);
                }
                else
                {
                    player.SendPacket(new PlayerKicked("An error occured while loading, Initial sync took too long to complete"));
                    PlayerDisconnected(player.Connection);
                    SendPacketToOtherPlayers(new Disconnect(player.Id), player);
                }
                timerData.Disposing = true;
                FinishProcessingReservation();
            }

            if (timerData.Counter >= timerData.MaxCounter)
            {
                Log.Error("An unexpected Error occured during InitialSync");
                PlayerDisconnected(timerData.Connection);

                timerData.Disposing = true;
                initialSyncTimer.Dispose(); // Looped long enough to require an override
            }

            timerData.Counter++;
        }
    }

    public void NonPlayerDisconnected(INitroxConnection connection)
    {
        // Remove any requests sent by the connection from the join queue
        JoinQueue = new(JoinQueue.Where(pair => !Equals(pair.Key, connection)));
    }

    public event Action<int>? PlayerCountChanged;

    public Player PlayerConnected(INitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
    {
        PlayerContext playerContext = reservations[reservationKey];
        Validate.NotNull(playerContext);

        ConnectionAssets assetPackage = assetsByConnection[connection];
        Validate.NotNull(assetPackage);

        wasBrandNewPlayer = playerContext.WasBrandNewPlayer;

        if (!allPlayersByName.TryGetValue(playerContext.PlayerName, out Player player))
        {
            player = new Player(playerContext.PlayerId,
                playerContext.PlayerName,
                false,
                playerContext,
                connection,
                NitroxVector3.Zero,
                NitroxQuaternion.Identity,
                playerContext.PlayerNitroxId,
                Optional.Empty,
                serverConfig.DefaultPlayerPerm,
                serverConfig.DefaultPlayerStats,
                serverConfig.GameMode,
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

        connectedPlayersById.Add(playerContext.PlayerId, player);

        // TODO: make a ConnectedPlayer wrapper so this is not stateful
        player.PlayerContext = playerContext;
        player.Connection = connection;

        // reconnecting players need to have their cell visibility refreshed
        player.ClearVisibleCells();

        assetPackage.Player = player;
        assetPackage.ReservationKey = null;
        reservations.Remove(reservationKey);

        PlayerCountChanged?.Invoke(connectedPlayersById.Count);

        return player;
    }

    public void PlayerDisconnected(INitroxConnection connection)
    {
        assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
        if (assetPackage == null)
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
            Player player = assetPackage.Player;
            reservedPlayerNames.Remove(player.Name);
            connectedPlayersById.Remove(player.Id);
            Log.Info($"{player.Name} left the game");
        }

        assetsByConnection.Remove(connection);

        PlayerCountChanged?.Invoke(connectedPlayersById.Count);

        if (!ConnectedPlayers().Any())
        {
            Server.Instance.PauseServer();
            Server.Instance.Save();
        }
    }

    public void FinishProcessingReservation(Player player = null)
    {
        initialSyncTimer.Dispose();
        PlayerCurrentlyJoining = false;
        if (player != null)
        {
            BroadcastPlayerJoined(player);
        }

        Log.Info($"Finished processing reservation. Remaining requests: {JoinQueue.Count}");

        // Tell next client that it can start joining.
        if (JoinQueue.Count > 0)
        {
            KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest> keyValuePair = JoinQueue.Dequeue();
            INitroxConnection requestConnection = keyValuePair.Key;
            MultiplayerSessionReservationRequest reservationRequest = keyValuePair.Value;

            MultiplayerSessionReservation reservation = ReservePlayerContext(requestConnection,
            reservationRequest.PlayerSettings,
            reservationRequest.AuthenticationContext,
            reservationRequest.CorrelationId);

            requestConnection.SendPacket(reservation);
        }
    }

    public bool TryGetPlayerByName(string playerName, out Player foundPlayer)
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

    public bool TryGetPlayerById(ushort playerId, out Player player)
    {
        return connectedPlayersById.TryGetValue(playerId, out player);
    }

    public Player GetPlayer(INitroxConnection connection)
    {
        if (!assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage))
        {
            return null;
        }
        return assetPackage.Player;
    }

    public Optional<Player> GetPlayer(string playerName)
    {
        allPlayersByName.TryGetValue(playerName, out Player player);
        return Optional.OfNullable(player);
    }

    public void SendPacketToAllPlayers(Packet packet)
    {
        foreach (Player player in ConnectedPlayers())
        {
            player.SendPacket(packet);
        }
    }

    public void SendPacketToOtherPlayers(Packet packet, Player sendingPlayer)
    {
        foreach (Player player in ConnectedPlayers())
        {
            if (player != sendingPlayer)
            {
                player.SendPacket(packet);
            }
        }
    }

    public IEnumerable<Player> ConnectedPlayers()
    {
        return assetsByConnection.Values
            .Where(assetPackage => assetPackage.Player != null)
            .Select(assetPackage => assetPackage.Player);
    }

    public void BroadcastPlayerJoined(Player player)
    {
        PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, player.Entity);
        SendPacketToOtherPlayers(playerJoinedPacket, player);
    }

}
