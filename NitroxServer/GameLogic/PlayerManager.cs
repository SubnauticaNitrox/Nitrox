using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;
using NitroxModel.PlayerSlot;
using NitroxModel.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxServer.GameLogic
{
    public class PlayerManager
    {
        private readonly HashSet<string> currentPlayerNames = new HashSet<string>();
        private readonly Dictionary<string, PlayerSlotReservation> reservations = new Dictionary<string, PlayerSlotReservation>();
        private readonly Dictionary<Connection, Player> playersByConnection = new Dictionary<Connection, Player>();
        
        public List<Player> GetPlayers()
        {
            lock (playersByConnection)
            {
                return playersByConnection.Values.ToList();
            }
        }

        public PlayerSlotReservation ReservePlayerSlot(string correlationId, string playerId)
        {
            if (currentPlayerNames.Contains(playerId))
            {
                PlayerSlotReservationState rejectedReservationState = PlayerSlotReservationState.Rejected | PlayerSlotReservationState.PlayerNameInUse;
                return new PlayerSlotReservation(correlationId, rejectedReservationState);
            }

            string reservationKey = Guid.NewGuid().ToString();
            PlayerSlotReservation reservation = new PlayerSlotReservation(correlationId, reservationKey, playerId);

            lock (reservations)
            {
                reservations.Add(reservationKey, reservation);
            }

            return reservation;
        }

        public Player ClaimPlayerSlotReservation(Connection connection, string reservationKey, string correlationId)
        {
            PlayerSlotReservation reservation = reservations[reservationKey];
            Validate.NotNull(reservation);

            if (reservation.CorrelationId != correlationId)
            {
                throw new UncorrelatedMessageException(); ;
            }

            Player player = new Player(reservation.PlayerId, connection);
            lock (playersByConnection)
            {
                lock (currentPlayerNames)
                {
                    currentPlayerNames.Add(reservation.PlayerId);
                }

                playersByConnection[connection] = player;

                lock (reservations)
                {
                    reservations.Remove(reservationKey);
                }
            }

            return player;
        }

        public void PlayerDisconnected(Connection connection)
        {
            lock (currentPlayerNames)
            {
                currentPlayerNames.Remove(playersByConnection[connection].Id);
            }

            lock (playersByConnection)
            {
                playersByConnection.Remove(connection);
            }
        }
        
        public Player GetPlayer(Connection connection)
        {
            Player player = null;

            lock (playersByConnection)
            {
                playersByConnection.TryGetValue(connection, out player);                    
            }

            return player;
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            lock (playersByConnection)
            {
                foreach(Player player in playersByConnection.Values)
                {
                    player.SendPacket(packet);
                }
            }
        }

        public void SendPacketToOtherPlayers(Packet packet, Player sendingPlayer)
        {
            lock (playersByConnection)
            {
                foreach (Player player in playersByConnection.Values)
                {
                    if (player != sendingPlayer)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }        
    }
}
