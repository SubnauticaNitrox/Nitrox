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
using NitroxServer.ConfigParser;

namespace NitroxServer.GameLogic
{
    // TODO: These methods a a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly Dictionary<NitroxConnection, ConnectionAssets> assetsByConnection = new Dictionary<NitroxConnection, ConnectionAssets>();
        private readonly ServerConfig serverConfig;
        private readonly Dictionary<string, PlayerContext> reservations = new Dictionary<string, PlayerContext>();
        private readonly HashSet<string> reservedPlayerNames = new HashSet<string>();
        private readonly Dictionary<string, Player> allPlayersByName;
        private ushort currentPlayerId = 0;

        public PlayerManager(List<Player> players, ServerConfig serverConfig)
        {
            allPlayersByName = players.ToDictionary(x => x.Name);            
            currentPlayerId = (players.Count == 0) ? (ushort) 0 : players.Max(x => x.Id);

            this.serverConfig = serverConfig;
        }

        public List<Player> GetConnectedPlayers()
        {
            lock (assetsByConnection)
            {
                return ConnectedPlayers();
            }
        }

        public List<Player> GetAllPlayers()
        {
            lock(allPlayersByName)
            {
                return new List<Player>(allPlayersByName.Values);
            }
        }

        public MultiplayerSessionReservation ReservePlayerContext(
            NitroxConnection connection,
            PlayerSettings playerSettings,
            AuthenticationContext authenticationContext,
            string correlationId)
        {
            lock (assetsByConnection)
            {
                // TODO: ServerPassword in NitroxClient

                if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (authenticationContext.ServerPassword.IsEmpty() || (authenticationContext.ServerPassword.Get() != serverConfig.ServerPassword)))
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
                if (reservedPlayerNames.Contains(playerName))
                {
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
                    return new MultiplayerSessionReservation(correlationId, rejectedState);
                }

                ConnectionAssets assetPackage;
                assetsByConnection.TryGetValue(connection, out assetPackage);
                if (assetPackage == null)
                {
                    assetPackage = new ConnectionAssets();
                    assetsByConnection.Add(connection, assetPackage);
                    reservedPlayerNames.Add(playerName);
                }

                Player player;
                allPlayersByName.TryGetValue(playerName, out player);

                bool hasSeenPlayerBefore = player != null;
                ushort playerId = (hasSeenPlayerBefore) ? player.Id : ++currentPlayerId;

                PlayerContext playerContext = new PlayerContext(playerName, playerId, !hasSeenPlayerBefore, playerSettings);
                string reservationKey = Guid.NewGuid().ToString();

                reservations.Add(reservationKey, playerContext);
                assetPackage.ReservationKey = reservationKey;

                return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
            }
        }

        public Player PlayerConnected(NitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = assetsByConnection[connection];
                PlayerContext playerContext = reservations[reservationKey];
                Validate.NotNull(playerContext);

                wasBrandNewPlayer = playerContext.WasBrandNewPlayer;
                
                Player player;
                
                lock (allPlayersByName)
                {
                    if (!allPlayersByName.TryGetValue(playerContext.PlayerName, out player))
                    {
                        player = new Player(playerContext.PlayerId, playerContext.PlayerName, playerContext, connection, NitroxVector3.Zero, new NitroxId(), Optional<NitroxId>.Empty(), Perms.PLAYER, new PlayerStatsData(), new List<EquippedItemData>(), new List<EquippedItemData>());
                        allPlayersByName[playerContext.PlayerName] = player;
                    }
                }

                // TODO: make a ConnectedPlayer wrapper so this is not stateful
                player.PlayerContext = playerContext;
                player.connection = connection;

                assetPackage.Player = player;
                assetPackage.ReservationKey = null;
                reservations.Remove(reservationKey);

                return player;
            }
        }

        public void PlayerDisconnected(NitroxConnection connection)
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
        
        public bool TryGetPlayerByName(string playerName, out Player foundPlayer)
        {
            lock (assetsByConnection)
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
        }

        public Player GetPlayer(NitroxConnection connection)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = null;
                assetsByConnection.TryGetValue(connection, out assetPackage);
                return assetPackage?.Player;
            }
        }

        public Optional<Player> GetPlayer(string playerName)
        {
            lock (allPlayersByName)
            {
                Player player;
                allPlayersByName.TryGetValue(playerName, out player);

                return Optional<Player>.OfNullable(player);
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
