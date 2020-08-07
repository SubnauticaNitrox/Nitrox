using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication.NetworkingLayer;

namespace NitroxServer.GameLogic
{
    // TODO: These methods a a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly ThreadSafeDictionary<NitroxConnection, ConnectionAssets> assetsByConnection;
        private readonly ThreadSafeDictionary<string, PlayerContext> reservations;
        private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
        private readonly ThreadSafeCollection<string> reservedPlayerNames;

        private readonly PlayerStatsData defaultPlayerStats;
        private readonly ServerConfig serverConfig;
        private ushort currentPlayerId;

        public PlayerManager(List<Player> players, ServerConfig serverConfig)
        {
            allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
            assetsByConnection = new ThreadSafeDictionary<NitroxConnection, ConnectionAssets>();
            reservedPlayerNames = new ThreadSafeCollection<string>(new HashSet<string>());
            reservations = new ThreadSafeDictionary<string, PlayerContext>();

            currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);
            defaultPlayerStats = serverConfig.DefaultPlayerStats;

            this.serverConfig = serverConfig;
        }

        public List<Player> GetConnectedPlayers()
        {
            return assetsByConnection.Values
                .Where(assetPackage => assetPackage.Player != null)
                .Select(assetPackage => assetPackage.Player).ToList();
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return allPlayersByName.Values;
        }

        public Player GetPlayer(NitroxConnection connection)
        {
            assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
            return assetPackage?.Player;
        }

        public bool TryGetPlayerByName(string playerName, out Player foundPlayer)
        {
            List<Player> players = GetConnectedPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Name == playerName)
                {
                    foundPlayer = players[i];
                    return true;
                }
            }

            foundPlayer = null;
            return false;
        }

        public MultiplayerSessionReservation ReservePlayerContext(NitroxConnection connection, PlayerSettings playerSettings, AuthenticationContext authenticationContext, string correlationId)
        {
            if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != serverConfig.ServerPassword))
            {
                MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
                return new MultiplayerSessionReservation(correlationId, rejectedState);
            }

            if (reservedPlayerNames.Count >= serverConfig.MaxConnections)
            {
                MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
                return new MultiplayerSessionReservation(correlationId, rejectedState);
            }

            string playerName = authenticationContext.Username;
            allPlayersByName.TryGetValue(playerName, out Player player);

            if ((player?.IsPermaDeath == true) && serverConfig.IsHardcore)
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

            PlayerContext playerContext = new PlayerContext(playerName, playerId, !hasSeenPlayerBefore, playerSettings);
            string reservationKey = Guid.NewGuid().ToString();

            reservations.Add(reservationKey, playerContext);
            assetPackage.ReservationKey = reservationKey;

            if (GetConnectedPlayers().Count == 1)
            {
                Server.Instance.EnablePeriodicSaving();
            }

            return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
        }

        public Player PlayerConnected(NitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
        {
            PlayerContext playerContext = reservations[reservationKey];
            ConnectionAssets assetPackage = assetsByConnection[connection];

            Validate.NotNull(playerContext);
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
                    new NitroxId(),
                    Optional.Empty,
                    Perms.PLAYER,
                    defaultPlayerStats,
                    new List<EquippedItemData>(),
                    new List<EquippedItemData>());

                allPlayersByName[playerContext.PlayerName] = player;
            }

            // TODO: make a ConnectedPlayer wrapper so this is not stateful
            player.PlayerContext = playerContext;
            player.connection = connection;

            assetPackage.Player = player;
            assetPackage.ReservationKey = null;
            reservations.Remove(reservationKey);

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

                if (GetConnectedPlayers().Count == 0)
                {
                    Server.Instance.DisablePeriodicSaving();
                    Server.Instance.Save();
                }
            }

            assetsByConnection.Remove(connection);
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            List<Player> players = GetConnectedPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].SendPacket(packet);
            }
        }

        public void SendPacketToOtherPlayers(Packet packet, Player sendingPlayer)
        {
            List<Player> players = GetConnectedPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != sendingPlayer)
                {
                    players[i].SendPacket(packet);
                }
                
            }
        }
    }
}
