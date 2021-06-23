using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication.NetworkingLayer;
using NitroxServer.Serialization;

namespace NitroxServer.GameLogic
{
    // TODO: These methods are a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly ThreadSafeDictionary<string, Player> allPlayersByName;
        private readonly ThreadSafeDictionary<NitroxConnection, ConnectionAssets> assetsByConnection = new();
        private readonly ThreadSafeDictionary<string, PlayerContext> reservations = new();
        private readonly ThreadSafeCollection<string> reservedPlayerNames = new ThreadSafeCollection<string>(new HashSet<string> { "Player" }); // "Player" is often used to identify the local player and should not be used by any user

        private readonly ServerConfig serverConfig;
        private ushort currentPlayerId;

        public PlayerManager(List<Player> players, ServerConfig serverConfig)
        {
            allPlayersByName = new ThreadSafeDictionary<string, Player>(players.ToDictionary(x => x.Name), false);
            currentPlayerId = players.Count == 0 ? (ushort)0 : players.Max(x => x.Id);

            this.serverConfig = serverConfig;
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
            string correlationId)
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
            NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();

            PlayerContext playerContext = new PlayerContext(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings);
            string reservationKey = Guid.NewGuid().ToString();

            reservations.Add(reservationKey, playerContext);
            assetPackage.ReservationKey = reservationKey;

            return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
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
                    playerContext.PlayerNitroxId,
                    Optional.Empty,
                    serverConfig.DefaultPlayerPerm,
                    serverConfig.DefaultPlayerStats,
                    new List<NitroxTechType>(),
                    Array.Empty<string>(),
                    new List<EquippedItemData>(),
                    new List<EquippedItemData>()
                );
                allPlayersByName[playerContext.PlayerName] = player;
            }

            // TODO: make a ConnectedPlayer wrapper so this is not stateful
            player.PlayerContext = playerContext;
            player.connection = connection;

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

            if (ConnectedPlayers().Count() == 0)
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
