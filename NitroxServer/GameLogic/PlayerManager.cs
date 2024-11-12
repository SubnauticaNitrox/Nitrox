using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication;
using NitroxServer.GameLogic.Bases;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.GameLogic
{
    // TODO: These methods are a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly World world;

        private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
        private readonly ThreadSafeDictionary<INitroxConnection, ConnectionAssets> assetsByConnection = [];
        private readonly ThreadSafeDictionary<string, PlayerContext> reservations = [];
        private readonly ThreadSafeSet<string> reservedPlayerNames = new("Player"); // "Player" is often used to identify the local player and should not be used by any user

        private ThreadSafeQueue<(INitroxConnection, string)> JoinQueue { get; set; } = new();
        private bool queueIdle = false;
        public Action SyncFinishedCallback { get; private set; }

        private readonly ServerConfig serverConfig;
        private ushort currentPlayerId;

        public PlayerManager(List<Player> players, World world, ServerConfig serverConfig)
        {
            allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
            currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);

            this.world = world;
            this.serverConfig = serverConfig;

            _ = JoinQueueLoop();
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

        public IEnumerable<INitroxConnection> GetQueuedPlayers()
        {
            return JoinQueue.Select(tuple => tuple.Item1);
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
            NitroxGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : serverConfig.GameMode;

            // TODO: At some point, store the muted state of a player
            PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode);
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
                    while (JoinQueue.Count == 0)
                    {
                        queueIdle = true;
                        await Task.Delay(REFRESH_DELAY);
                    }

                    queueIdle = false;

                    (INitroxConnection connection, string reservationKey) = JoinQueue.Dequeue();
                    string name = reservations[reservationKey].PlayerName;

                    // Do this after dequeueing because everyone's position shifts forward
                    {
                        (INitroxConnection, string)[] array = [.. JoinQueue];
                        for (int i = 0; i < array.Length; i++)
                        {
                            (INitroxConnection c, _) = array[i];
                            c.SendPacket(new JoinQueueInfo(i + 1, serverConfig.InitialSyncTimeout, false));
                        }
                    }

                    Log.Info($"Starting sync for player {name}");
                    SendInitialSync(connection, reservationKey);

                    CancellationTokenSource source = new(serverConfig.InitialSyncTimeout);
                    bool syncFinished = false;
                    bool disconnected = false;

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
                            else if (connection.State == NitroxConnectionState.Disconnected)
                            {
                                disconnected = true;
                                return false;
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

                        if (disconnected)
                        {
                            Log.Info($"Player {name} disconnected while syncing");
                            return;
                        }

                        if (task.IsCanceled || !task.Result)
                        {
                            Log.Info($"Initial sync timed out for player {name}");
                            SyncFinishedCallback = null;

                            if (connection.State == NitroxConnectionState.Connected)
                            {
                                connection.SendPacket(new PlayerSyncTimeout());
                            }
                            PlayerDisconnected(connection);
                        }
                        else
                        {
                            Log.Info($"Player {name} joined successfully. Remaining requests: {JoinQueue.Count}");
                            BroadcastPlayerJoined(assetsByConnection[connection].Player);
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Unexpected error during player connection: {e}");
                }
            }
        }

        public void AddToJoinQueue(INitroxConnection connection, string reservationKey)
        {
            if (!queueIdle)
            {
                connection.SendPacket(new JoinQueueInfo(JoinQueue.Count + 1, serverConfig.InitialSyncTimeout, true));
            }

            Log.Info($"Added player {reservations[reservationKey].PlayerName} to queue");
            JoinQueue.Enqueue((connection, reservationKey));
        }

        private void SendInitialSync(INitroxConnection connection, string reservationKey)
        {
            IEnumerable<PlayerContext> GetOtherPlayers(Player player)
            {
                return GetConnectedPlayers().Where(p => p != player)
                                                          .Select(p => p.PlayerContext);
            }

            PlayerWorldEntity SetupPlayerEntity(Player player)
            {
                NitroxTransform transform = new(player.Position, player.Rotation, NitroxVector3.One);

                PlayerWorldEntity playerEntity = new PlayerWorldEntity(transform, 0, null, false, player.GameObjectId, NitroxTechType.None, null, null, []);
                world.EntityRegistry.AddOrUpdate(playerEntity);
                world.WorldEntityManager.TrackEntityInTheWorld(playerEntity);
                return playerEntity;
            }

            PlayerWorldEntity RespawnExistingEntity(Player player)
            {
                if (world.EntityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId,
                    out PlayerWorldEntity playerWorldEntity))
                {
                    return playerWorldEntity;
                }

                Log.Error($"Unable to find player entity for {player.Name}");
                return SetupPlayerEntity(player);
            }

            Player player = PlayerConnected(connection, reservationKey, out bool wasBrandNewPlayer);
            NitroxId assignedEscapePodId = world.EscapePodManager.AssignPlayerToEscapePod(player.Id, out Optional<EscapePodWorldEntity> newlyCreatedEscapePod);

            if (newlyCreatedEscapePod.HasValue)
            {
                SpawnEntities spawnNewEscapePod = new(newlyCreatedEscapePod.Value);
                SendPacketToOtherPlayers(spawnNewEscapePod, player);
            }

            List<EquippedItemData> equippedItems = player.GetEquipment();

            // Make players on localhost admin by default.
            if (IPAddress.IsLoopback(connection.Endpoint.Address))
            {
                player.Permissions = Perms.ADMIN;
            }

            List<SimulatedEntity> simulations = world.EntitySimulation.AssignGlobalRootEntitiesAndGetData(player);

            player.Entity = wasBrandNewPlayer ? SetupPlayerEntity(player) : RespawnExistingEntity(player);

            List<GlobalRootEntity> globalRootEntities = world.WorldEntityManager.GetGlobalRootEntities(true);
            bool isFirstPlayer = GetConnectedPlayers().Count == 1;

            InitialPlayerSync initialPlayerSync = new(player.GameObjectId,
                wasBrandNewPlayer,
                assignedEscapePodId,
                equippedItems,
                player.UsedItems,
                player.QuickSlotsBindingIds,
                world.GameData.PDAState.GetInitialPDAData(),
                world.GameData.StoryGoals.GetInitialStoryGoalData(world.ScheduleKeeper, player),
                player.Position,
                player.Rotation,
                player.SubRootId,
                player.Stats,
                GetOtherPlayers(player),
                globalRootEntities,
                simulations,
                world.GameMode,
                player.Permissions,
                new(new(player.PingInstancePreferences), player.PinnedRecipePreferences.ToList()),
                world.StoryManager.GetTimeData(),
                isFirstPlayer,
                BuildingManager.GetEntitiesOperations(globalRootEntities)
            );

            player.SendPacket(initialPlayerSync);
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
                    new List<NitroxTechType>(),
                    Array.Empty<Optional<NitroxId>>(),
                    new List<EquippedItemData>(),
                    new List<EquippedItemData>(),
                    new Dictionary<string, float>(),
                    new Dictionary<string, PingInstancePreference>(),
                    new List<int>()
                );
                allPlayersByName[playerContext.PlayerName] = player;
            }

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

        public void NonPlayerDisconnected(INitroxConnection connection)
        {
            // They may have been queued, so just erase their entry
            JoinQueue = new(JoinQueue.Where(tuple => !Equals(tuple.Item1, connection)));
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

        private IEnumerable<Player> ConnectedPlayers()
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
}
