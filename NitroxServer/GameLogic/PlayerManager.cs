﻿using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Tcp;

namespace NitroxServer.GameLogic
{
    //TODO: These methods a a little chunky. Need to look at refactoring just to clean them up and get them around 30 lines a piece.
    public class PlayerManager
    {
        private readonly HashSet<string> reservedPlayerNames = new HashSet<string>();
        private readonly Dictionary<string, PlayerContext> reservations = new Dictionary<string, PlayerContext>();
        private readonly Dictionary<Connection, ConnectionAssets> assetsByConnection = new Dictionary<Connection, ConnectionAssets>();
        
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

                lock (reservedPlayerNames)
                {
                    if (reservedPlayerNames.Contains(playerName))
                    {
                        MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniquePlayerNameConstraintViolated;
                        return new MultiplayerSessionReservation(correlationId, rejectedState);
                    }

                    reservedPlayerNames.Add(playerName);
                }

                PlayerContext playerContext = new PlayerContext(playerName, playerSettings);
                string playerId = playerContext.PlayerId;
                string reservationKey;

                lock (reservations)
                {
                    reservationKey = Guid.NewGuid().ToString();
                    reservations.Add(reservationKey, playerContext);
                }

                assetPackage.ReservationKey = reservationKey;

                return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
            }
        }

        public Player CreatePlayer(Connection connection, string reservationKey)
        {
            lock (assetsByConnection)
            {
                ConnectionAssets assetPackage = assetsByConnection[connection];

                lock (reservations)
                {
                    PlayerContext playerContext = reservations[reservationKey];
                    Validate.NotNull(playerContext);

                    Player player = new Player(playerContext, connection);
                    assetPackage.Player = player;
                    assetPackage.ReservationKey = null;
                    reservations.Remove(reservationKey);

                    return player;
                }
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

                lock (reservedPlayerNames)
                {
                    if (assetPackage.ReservationKey != null)
                    {
                        lock (reservations)
                        {
                            PlayerContext playerContext = reservations[assetPackage.ReservationKey];
                            reservedPlayerNames.Remove(playerContext.PlayerName);
                            reservations.Remove(assetPackage.ReservationKey);
                        }
                    }

                    if (assetPackage.Player != null)
                    {
                        Player player = assetPackage.Player;
                        reservedPlayerNames.Remove(player.Name);
                    }
                }

                assetsByConnection.Remove(connection);
            }
        }

        public Player GetPlayer(Connection connection)
        {
            ConnectionAssets assetPackage = null;

            lock (assetsByConnection)
            {
                assetsByConnection.TryGetValue(connection, out assetPackage);
            }

            return assetPackage?.Player;
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
