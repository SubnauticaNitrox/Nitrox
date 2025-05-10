using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Respositories;

internal sealed class PlayerRepository(DatabaseService databaseService) : IPlayerRepository
{
    private readonly DatabaseService databaseService = databaseService;

    public async Task<PlayContext> GetPlayContextById(SessionId sessionId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayContexts
                       .AsNoTracking()
                       .Include(p => p.Session)
                       .Include(p => p.Session.Player)
                       .Where(p => p.Session.Id == sessionId && p.Session.Active)
                       .FirstOrDefaultAsync();
    }

    public async Task<ConnectedPlayerDto[]> GetConnectedPlayersAsync()
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayerSessions
                       .Include(p => p.Player)
                       .Where(p => p.Active)
                       .Select(s => s.ToConnectedPlayerDto())
                       .ToArrayAsync();
    }

    /// <summary>
    ///     Gets the players with the given name. Since multiple players can share the same name, returns an array.
    /// </summary>
    public async Task<ConnectedPlayerDto[]> GetConnectedPlayersByNameAsync(string name)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayerSessions
                       .Include(p => p.Player)
                       .Where(p => p.Active)
                       .Where(p => p.Player.Name == name)
                       .Select(p => p.ToConnectedPlayerDto())
                       .ToArrayAsync();
    }

    public async Task<ConnectedPlayerDto> GetConnectedPlayerByPlayerIdAsync(PeerId playerId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayerSessions
                       .Include(p => p.Player)
                       .Where(p => p.Active)
                       .Where(p => p.Player.Id == playerId)
                       .Select(p => p.ToConnectedPlayerDto())
                       .FirstOrDefaultAsync();
    }

    public async Task<ConnectedPlayerDto> GetConnectedPlayerBySessionIdAsync(SessionId sessionId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.PlayerSessions
                       .Include(p => p.Player)
                       .Where(p => p.Active)
                       .Where(p => p.Id == sessionId)
                       .Select(p => p.ToConnectedPlayerDto())
                       .FirstOrDefaultAsync();
    }

    public async Task<string> GetPlayerNameIfNotMuted(PeerId playerId)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        return await db.Players
                       .Where(p => p.Id == playerId && !p.IsMuted)
                       .Select(p => p.Name)
                       .FirstOrDefaultAsync();
    }

    public async Task<bool> SetPlayerPermissions(PeerId playerId, Perms permissions) => await SetPlayerProperty(playerId, permissions, (player, perms) => player.Permissions = perms);
    public async Task<bool> SetPlayerGameMode(PeerId playerId, SubnauticaGameMode gameMode) => await SetPlayerProperty(playerId, gameMode, (player, mode) => player.GameMode = mode);
    public async Task<bool> SetPlayerMuted(PeerId playerId, bool mute) => await SetPlayerProperty(playerId, mute, (player, m) => player.IsMuted = m);

    private async Task<bool> SetPlayerProperty<T>(PeerId playerId, T value, Action<Database.Models.Player, T> action)
    {
        await using WorldDbContext db = await databaseService.GetDbContextAsync();
        Database.Models.Player player = await db.Players
                                                .AsTracking()
                                                .FirstOrDefaultAsync(s => s.Id == playerId);
        if (player == null)
        {
            return false;
        }

        action(player, value);
        db.Players.Update(player);
        if (await db.SaveChangesAsync() != 1)
        {
            return false;
        }
        return true;
    }

    // TODO: Use DATABASE
    // private readonly ThreadSafeDictionary<string, PlayerContext> reservations = new();
    // - JOIN QUEUE

    // [Obsolete("DO NOT USE !!!")]
    // public MultiplayerSessionReservation ReservePlayerContext(
    //     SessionId sessionId,
    //     PlayerSettings playerSettings,
    //     AuthenticationContext authenticationContext,
    //     string correlationId)
    // {
    // if (reservedPlayerNames.Count >= serverConfig.MaxConnections)
    // {
    //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
    //     return new MultiplayerSessionReservation(correlationId, rejectedState);
    // }
    //
    // if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != serverConfig.ServerPassword))
    // {
    //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
    //     return new MultiplayerSessionReservation(correlationId, rejectedState);
    // }
    //
    // //https://regex101.com/r/eTWiEs/2/
    // if (!Regex.IsMatch(authenticationContext.Username, @"^[a-zA-Z0-9._-]{3,25}$"))
    // {
    //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
    //     return new MultiplayerSessionReservation(correlationId, rejectedState);
    // }
    //
    // if (PlayerCurrentlyJoining)
    // {
    //     if (JoinQueue.Any(pair => ReferenceEquals(pair.Key, connection)))
    //     {
    //         // Don't enqueue the request if there is already another enqueued request by the same user
    //         return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.REJECTED);
    //     }
    //
    //     JoinQueue.Enqueue(new KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>(
    //                           connection,
    //                           new MultiplayerSessionReservationRequest(correlationId, playerSettings, authenticationContext)));
    //
    //     return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE);
    // }
    //
    // string playerName = authenticationContext.Username;
    //
    // allPlayersByName.TryGetValue(playerName, out NitroxServer.Player player);
    // if (player?.IsPermaDeath == true && serverConfig.IsHardcore)
    // {
    //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.HARDCORE_PLAYER_DEAD;
    //     return new MultiplayerSessionReservation(correlationId, rejectedState);
    // }
    //
    // if (reservedPlayerNames.Contains(playerName))
    // {
    //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
    //     return new MultiplayerSessionReservation(correlationId, rejectedState);
    // }
    //
    // assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
    // if (assetPackage == null)
    // {
    //     assetPackage = new ConnectionAssets();
    //     assetsByConnection.Add(connection, assetPackage);
    //     reservedPlayerNames.Add(playerName);
    // }
    //
    // bool hasSeenPlayerBefore = player != null;
    // ushort playerId = hasSeenPlayerBefore ? player.Id : ++currentPlayerId;
    // NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();
    // SubnauticaGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : serverConfig.GameMode;
    // IntroCinematicMode introCinematicMode = hasSeenPlayerBefore ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;
    //
    // PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode);
    // string reservationKey = Guid.NewGuid().ToString();
    //
    // reservations.Add(reservationKey, playerContext);
    // assetPackage.ReservationKey = reservationKey;
    //
    // PlayerCurrentlyJoining = true;
    //
    // InitialSyncTimerData timerData = new(connection, authenticationContext, serverConfig.InitialSyncTimeout);
    // initialSyncTimer = new Timer(InitialSyncTimerElapsed, timerData, 0, 200);
    //
    // return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
    // }

    // [Obsolete("DO NOT USE !!!")]
    // public PeerId AddConnectedPlayer(SessionId session, string reservationKey, out bool wasBrandNewPlayer)
    // {
    //     PlayerContext playerContext = reservations[reservationKey];
    //     Validate.NotNull(playerContext);
    //     ConnectionAssets assetPackage = assetsByConnection[connection];
    //     Validate.NotNull(assetPackage);
    //
    //     wasBrandNewPlayer = playerContext.WasBrandNewPlayer;
    //
    //     if (!allPlayersByName.TryGetValue(playerContext.PlayerName, out NitroxPlayer player))
    //     {
    //         player = new NitroxPlayer(playerContext.PlayerId,
    //                                          playerContext.PlayerName,
    //                                          false,
    //                                          playerContext,
    //                                          connection,
    //                                          NitroxVector3.Zero,
    //                                          NitroxQuaternion.Identity,
    //                                          playerContext.PlayerNitroxId,
    //                                          Optional.Empty,
    //                                          serverConfig.DefaultPlayerPerm,
    //                                          serverConfig.DefaultPlayerStats,
    //                                          serverConfig.GameMode,
    //                                          new List<NitroxTechType>(),
    //                                          [],
    //                                          new Dictionary<string, NitroxId>(),
    //                                          new Dictionary<string, float>(),
    //                                          new Dictionary<string, PingInstancePreference>(),
    //                                          new List<int>()
    //         );
    //         allPlayersByName[playerContext.PlayerName] = player;
    //     }
    //
    //     connectedPlayersById.Add(playerContext.PlayerId, player);
    //
    //     player.PlayerContext = playerContext;
    //     player.Connection = connection;
    //
    //     // reconnecting players need to have their cell visibility refreshed
    //     player.ClearVisibleCells();
    //
    //     assetPackage.Player = player;
    //     assetPackage.ReservationKey = null;
    //     reservations.Remove(reservationKey);
    //
    //     return player;
    // }

    // public void FinishProcessingReservation(PeerId? player = null)
    // {
    //     initialSyncTimer.Dispose();
    //     PlayerCurrentlyJoining = false;
    //     if (player != null)
    //     {
    //         BroadcastPlayerJoined(player);
    //     }
    //
    //     Log.Info($"Finished processing reservation. Remaining requests: {JoinQueue.Count}");
    //
    //     // Tell next client that it can start joining.
    //     if (JoinQueue.Count > 0)
    //     {
    //         KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest> keyValuePair = JoinQueue.Dequeue();
    //         INitroxConnection requestConnection = keyValuePair.Key;
    //         MultiplayerSessionReservationRequest reservationRequest = keyValuePair.Value;
    //
    //         MultiplayerSessionReservation reservation = ReservePlayerContext(requestConnection,
    //                                                                          reservationRequest.PlayerSettings,
    //                                                                          reservationRequest.AuthenticationContext,
    //                                                                          reservationRequest.CorrelationId);
    //
    //         requestConnection.SendPacket(reservation);
    //     }
    // }

    // private void InitialSyncTimerElapsed(object state)
    // {
    //     if (state is InitialSyncTimerData { Disposing: false } timerData)
    //     {
    //         allPlayersByName.TryGetValue(timerData.Context.Username, out NitroxServer.Player player);
    //
    //         if (timerData.Connection.State < NitroxConnectionState.Connected)
    //         {
    //             if (player == null) // player can cancel the joining process before this timer elapses
    //             {
    //                 Log.Error("Player was nulled while joining");
    //                 Disconnect(timerData.Connection);
    //             }
    //             else
    //             {
    //                 player.SendPacket(new PlayerKicked("An error occured while loading, Initial sync took too long to complete"));
    //                 Disconnect(player.Connection);
    //                 SendPacketToOtherPlayers(new Disconnect(player.Id), player);
    //             }
    //             timerData.Disposing = true;
    //             FinishProcessingReservation();
    //         }
    //
    //         if (timerData.Counter >= timerData.MaxCounter)
    //         {
    //             Log.Error("An unexpected Error occured during InitialSync");
    //             Disconnect(timerData.Connection);
    //
    //             timerData.Disposing = true;
    //             initialSyncTimer.Dispose(); // Looped long enough to require an override
    //         }
    //
    //         timerData.Counter++;
    //     }
    // }

    // TODO: USE DATABASE
    // public void Teleport(ConnectedPlayerDto player, NitroxVector3 position)
    // {
    //     PlayerTeleported playerTeleported = new(player.Id, player.LastStoredPosition, position, player.LastStoredSubRootID);
    //     Position = playerTeleported.DestinationTo;
    //     LastStoredPosition = playerTeleported.DestinationFrom;
    //     LastStoredSubRootID = subRootID;
    //     SendPacket(playerTeleported, player.Id);
    // }
}
