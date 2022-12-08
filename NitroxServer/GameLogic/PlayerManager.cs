using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication;
using NitroxServer.Serialization;

namespace NitroxServer.GameLogic
{
    // TODO: These methods are a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
        private readonly ThreadSafeDictionary<NitroxConnection, ConnectionAssets> assetsByConnection = new();
        private readonly ThreadSafeDictionary<string, PlayerContext> reservations = new();
        private readonly ThreadSafeSet<string> reservedPlayerNames = new("Player"); // "Player" is often used to identify the local player and should not be used by any user

        private ThreadSafeQueue<KeyValuePair<NitroxConnection, MultiplayerSessionReservationRequest>> JoinQueue { get; set; } = new();
        public Action SyncFinishedCallback { get; private set; }

        private readonly ServerConfig serverConfig;
        private ushort currentPlayerId;

        public PlayerManager(List<Player> players, ServerConfig serverConfig)
        {
            allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
            currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);

            this.serverConfig = serverConfig;

            _ = JoinQueueLoop();
        }

        public List<Player> GetConnectedPlayers()
        {
            return ConnectedPlayers().ToList();
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return allPlayersByName.Values;
        }

        public MultiplayerSessionReservation ReservePlayerContext(
            NitroxConnection connection,
            PlayerSettings playerSettings,
            AuthenticationContext authenticationContext,
            string correlationId, 
            bool ignoreJoinQueue = false)
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

            if (!ignoreJoinQueue)
            {
                if (JoinQueue.Any(pair => ReferenceEquals(pair.Key, connection)))
                {
                    // Don't enqueue the request if there is already another enqueued request by the same user
                    return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.REJECTED);
                }
                
                JoinQueue.Enqueue(new KeyValuePair<NitroxConnection, MultiplayerSessionReservationRequest>(
                                      connection,
                                      new MultiplayerSessionReservationRequest(correlationId, playerSettings, authenticationContext)));

                return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE);
            }

            string playerName = authenticationContext.Username;

            allPlayersByName.TryGetValue(playerName, out Player player);
            if (player?.IsPermaDeath == true && serverConfig.IsHardcore)
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
            
            // TODO: At some point, store the muted state of a player
            PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false);
            string reservationKey = Guid.NewGuid().ToString();

            reservations.Add(reservationKey, playerContext);
            assetPackage.ReservationKey = reservationKey;

            return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
        }

        private async Task JoinQueueLoop()
        {
            const int REFRESH_DELAY = 10;

            while (true)
            {
                try
                {
                    while (!JoinQueue.Any())
                    {
                        await Task.Delay(REFRESH_DELAY);
                    }

                    var pair = JoinQueue.Dequeue();
                    NitroxConnection connection = pair.Key;
                    MultiplayerSessionReservationRequest request = pair.Value;


                    MultiplayerSessionReservation reservation = ReservePlayerContext(connection,
                        request.PlayerSettings,
                        request.AuthenticationContext,
                        request.CorrelationId, 
                        ignoreJoinQueue: true);

                    connection.SendPacket(reservation);

                    CancellationTokenSource source = new(serverConfig.InitialSyncTimeout);
                    bool syncFinished = false;

                    SyncFinishedCallback = () =>
                    {
                        syncFinished = true;
                    };

                    await Task.Run(() =>
                    {
                        while (!source.IsCancellationRequested)
                        {
                            if (syncFinished)
                            {
                                return true;
                            }
                            else
                            {
                                Task.Delay(REFRESH_DELAY).Wait();
                            }
                        }

                        return false;
                    
                    // We use ContinueWith to avoid having to try/catch a TaskCanceledException
                    }).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            throw task.Exception;
                        }

                        if (task.IsCanceled || !task.Result)
                        {
                            Log.Info($"Inital sync timed out for player {request.AuthenticationContext.Username}");
                            SyncFinishedCallback = null;

                            allPlayersByName.TryGetValue(request.AuthenticationContext.Username, out Player player);
                            if (connection.State == NitroxConnectionState.Connected)
                            {
                                connection.SendPacket(new PlayerKicked("An error occured while loading, initial sync took too long to complete"));
                            }
                            PlayerDisconnected(connection);
                        }
                        else
                        {
                            Log.Info($"Player {request.AuthenticationContext.Username} joined successfully. Remaining requests: {JoinQueue.Count}");
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Unexpected error during player connection: {e}");
                }
            }
        }

        public void NonPlayerDisconnected(NitroxConnection connection)
        {
            // Remove any requests sent by the connection from the join queue
            JoinQueue = new(JoinQueue.Where(pair => !Equals(pair.Key, connection)));
        }

        public Player PlayerConnected(NitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
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
                    new List<NitroxTechType>(),
                    Array.Empty<string>(),
                    new List<EquippedItemData>(),
                    new List<EquippedItemData>(),
                    new HashSet<string>(),
                    new Dictionary<string, PingInstancePreference>()
                );
                allPlayersByName[playerContext.PlayerName] = player;
            }

            // TODO: make a ConnectedPlayer wrapper so this is not stateful
            player.PlayerContext = playerContext;
            player.Connection = connection;

            assetPackage.Player = player;
            assetPackage.ReservationKey = null;
            reservations.Remove(reservationKey);

            if (ConnectedPlayers().Count() == 1)
            {
                Server.Instance.ResumeServer();
            }

            return player;
        }

        public void PlayerDisconnected(NitroxConnection connection)
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

        public Player GetPlayer(NitroxConnection connection)
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

        private IEnumerable<Player> ConnectedPlayers()
        {
            return assetsByConnection.Values
                .Where(assetPackage => assetPackage.Player != null)
                .Select(assetPackage => assetPackage.Player);
        }
    }
}
