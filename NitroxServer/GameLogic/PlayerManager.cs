using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication.NetworkingLayer;
using NitroxServer.ConfigParser;
using NitroxServer.GameLogic.Players;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic
{
    // TODO: These methods a a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly Dictionary<NitroxConnection, ConnectionAssets> assetsByConnection = new Dictionary<NitroxConnection, ConnectionAssets>();
        private readonly PlayerData playerData;
        private readonly ServerConfig serverConfig;
        private readonly Dictionary<string, PlayerContext> reservations = new Dictionary<string, PlayerContext>();
        private readonly HashSet<string> reservedPlayerNames = new HashSet<string>();

        public PlayerManager(PlayerData playerData, ServerConfig serverConfig)
        {
            this.playerData = playerData;
            this.serverConfig = serverConfig;
        }

        public List<Player> GetPlayers()
        {
            lock (assetsByConnection)
            {
                return ConnectedPlayers();
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
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.AuthenticationFailed;
                    return new MultiplayerSessionReservation(correlationId, rejectedState);
                }

                if (reservedPlayerNames.Count >= serverConfig.MaxConnections)
                {
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.ServerPlayerCapacityReached;
                    return new MultiplayerSessionReservation(correlationId, rejectedState);
                }

                string playerName = authenticationContext.Username;
                if (reservedPlayerNames.Contains(playerName))
                {
                    MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniquePlayerNameConstraintViolated;
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

                bool hasSeenPlayerBefore = playerData.hasSeenPlayerBefore(playerName);
                PlayerContext playerContext = new PlayerContext(playerName, playerData.GetPlayerId(playerName), !hasSeenPlayerBefore, playerSettings);
                NitroxId playerId = playerContext.PlayerId;
                string reservationKey = Guid.NewGuid().ToString();

                reservations.Add(reservationKey, playerContext);
                assetPackage.ReservationKey = reservationKey;

                return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
            }
        }

        public Player CreatePlayer(NitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = assetsByConnection[connection];
                PlayerContext playerContext = reservations[reservationKey];
                Validate.NotNull(playerContext);

                wasBrandNewPlayer = playerContext.WasBrandNewPlayer;

                // Load previously persisted data for this player.
                Vector3 position = playerData.GetPosition(playerContext.PlayerName);
                Optional<NitroxId> subRootId = playerData.GetSubRootId(playerContext.PlayerName);

                Player player = new Player(playerContext, connection, position, subRootId);
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
