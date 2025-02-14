using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.Communication;

namespace NitroxServer.GameLogic;

// TODO: These methods are a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
public class PlayerManager
{
    private readonly SubnauticaServerConfig serverConfig;

    private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
    private readonly ThreadSafeDictionary<ushort, Player> connectedPlayersById = [];
    private readonly ThreadSafeDictionary<INitroxConnection, ConnectionAssets> assetsByConnection = new();
    private readonly ThreadSafeDictionary<string, PlayerContext> reservations = new();
    private readonly ThreadSafeSet<string> reservedPlayerNames = new("Player"); // "Player" is often used to identify the local player and should not be used by any user

    private ushort currentPlayerId;

    public PlayerManager(List<Player> players, SubnauticaServerConfig serverConfig)
    {
        allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
        currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);

        this.serverConfig = serverConfig;
    }

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

    public Player GetPlayer(INitroxConnection connection)
    {
        return assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage) ? assetPackage.Player : null;
    }

    public PlayerContext GetPlayerContext(string reservationKey)
    {
        return reservations.TryGetValue(reservationKey, out PlayerContext playerContext) ? playerContext : null;
    }

    public MultiplayerSessionReservation ReservePlayerContext(
        INitroxConnection connection,
        PlayerSettings playerSettings,
        AuthenticationContext authenticationContext,
        string correlationId)
    {
        if (reservedPlayerNames.Count >= serverConfig.MaxConnections)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != serverConfig.ServerPassword))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        //https://regex101.com/r/eTWiEs/2/
        if (!Regex.IsMatch(authenticationContext.Username, @"^[a-zA-Z0-9._-]{3,25}$"))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
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

        // TODO: At some point, store the muted state of a player
        PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode);
        string reservationKey = Guid.NewGuid().ToString();

        reservations.Add(reservationKey, playerContext);
        assetPackage.ReservationKey = reservationKey;

        return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
    }

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
                []
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

        return player;
    }

    public void PlayerDisconnected(INitroxConnection connection)
    {
        if (!assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage))
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
        }

        assetsByConnection.Remove(connection);

        if (!ConnectedPlayers().Any())
        {
            Server.Instance.PauseServer();
            Server.Instance.Save();
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
}
