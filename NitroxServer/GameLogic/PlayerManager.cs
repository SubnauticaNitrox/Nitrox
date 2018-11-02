using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Players;
using static NitroxServer.GameLogic.Players.PlayerData;
using NitroxServer.Communication;

namespace NitroxServer.GameLogic
{
    // TODO: These methods a a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly HashSet<string> reservedPlayerNames = new HashSet<string>();
        private readonly Dictionary<string, PlayerContext> reservations = new Dictionary<string, PlayerContext>();
        private readonly Dictionary<Connection, ConnectionAssets> assetsByConnection = new Dictionary<Connection, ConnectionAssets>();
        private readonly PlayerData playerData;
        private readonly bool allowAuthenticationOfDuplicateSteamIds = true;

        public PlayerManager(PlayerData playerData)
        {
            this.playerData = playerData;
        }

        public List<Player> GetPlayers()
        {
            lock (assetsByConnection)
            {
                return ConnectedPlayers();
            }
        }

        public MultiplayerSessionReservation ReservePlayerContext(
            Connection connection,
            PlayerSettings playerSettings,
            AuthenticationContext authenticationContext,
            string correlationId)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage;
                assetsByConnection.TryGetValue(connection, out assetPackage);

                if (assetPackage == null)
                {
                    assetPackage = new ConnectionAssets();
                    assetsByConnection.Add(connection, assetPackage);
                }

                string playerName = authenticationContext.Username;

                if (reservedPlayerNames.Contains(playerName))
                {
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniquePlayerNameConstraintViolated;
                    return new MultiplayerSessionReservation(correlationId, rejectedState);
                }

                ulong playId;
                if (ResolveDuplicateSteamIds(authenticationContext, correlationId, out playId))
                {
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniqueSteamIdConstraintViolated;
                    return new MultiplayerSessionReservation(correlationId, rejectedState);
                }

                reservedPlayerNames.Add(playerName);

                PlayerContext playerContext = new PlayerContext(playerName, playId, playerSettings);
                ulong playerId = playerContext.PlayerId;
                string reservationKey = playerContext.PlayerId.ToString();

                reservations.Add(reservationKey, playerContext);
                assetPackage.ReservationKey = reservationKey;

                return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
            }
        }

        bool ResolveDuplicateSteamIds(AuthenticationContext authenticationContext, string correlationId, out ulong playId)
        {
            playId = authenticationContext.SteamId;
            if (allowAuthenticationOfDuplicateSteamIds) // Duplicate steam ids will be emulated as seperate players
            {
                PlayerContext context;

                if (reservations.TryGetValue(playId.ToString(), out context))
                {
                    playId += (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
                }
            }
            else
            {
                PlayerContext context;

                if (reservations.TryGetValue(playId.ToString(), out context))
                {
                    return true;
                }
            }
            return false;
        }

        public Player CreatePlayer(Connection connection, string reservationKey)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = assetsByConnection[connection];
                PlayerContext playerContext = reservations[reservationKey];
                Validate.NotNull(playerContext);

                Player player = new Player(playerContext, connection);
                assetPackage.Player = player;
                assetPackage.ReservationKey = null;
                reservations.Remove(reservationKey);
                
                return player;
            }
        }

        public void PlayerDisconnected(Connection connection)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = null;
                assetsByConnection.TryGetValue(connection, out assetPackage);

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
            }
        }

        public Player GetPlayer(Connection connection)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = null;
                assetsByConnection.TryGetValue(connection, out assetPackage);
                return assetPackage?.Player;
            }
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            lock (assetsByConnection)
            {
                foreach (Player player in ConnectedPlayers())
                {
                    player.SendPacket(packet);
                }
            }
        }

        public void SendPacketToOtherPlayers(Packet packet, Player sendingPlayer)
        {
            lock (assetsByConnection)
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

        private List<Player> ConnectedPlayers()
        {
            lock (assetsByConnection)
            {
                return assetsByConnection.Values
                    .Where(assetPackage => assetPackage.Player != null)
                    .Select(assetPackage => assetPackage.Player)
                    .ToList();
            }
        }
    }
}
