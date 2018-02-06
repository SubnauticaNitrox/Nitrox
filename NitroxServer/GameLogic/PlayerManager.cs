using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;
using NitroxModel.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.MultiplayerSession;

namespace NitroxServer.GameLogic
{
    public class PlayerManager
    {
        private readonly HashSet<string> reservedPlayerNames = new HashSet<string>();
        private readonly Dictionary<string, MultiplayerSessionReservation> reservations = new Dictionary<string, MultiplayerSessionReservation>();
        private readonly Dictionary<Connection, Player> playersByConnection = new Dictionary<Connection, Player>();
        
        public List<Player> GetPlayers()
        {
            lock (playersByConnection)
            {
                return playersByConnection.Values.ToList();
            }
        }

        public MultiplayerSessionReservation ReservePlayerSlot(string correlationId, string playerId)
        {
            lock (reservedPlayerNames)
            {
                if (reservedPlayerNames.Contains(playerId))
                {
                    MultiplayerSessionReservationState rejectedReservationState = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniquePlayerNameConstraintViolated;
                    return new MultiplayerSessionReservation(correlationId, rejectedReservationState);
                }

                reservedPlayerNames.Add(playerId);
            }            

            string reservationKey = Guid.NewGuid().ToString();
            MultiplayerSessionReservation reservation = new MultiplayerSessionReservation(correlationId, reservationKey, playerId);

            lock (reservations)
            {
                reservations.Add(reservationKey, reservation);
            }

            return reservation;
        }

        public Player ClaimPlayerSlotReservation(Connection connection, string reservationKey, string correlationId)
        {
            MultiplayerSessionReservation reservation = reservations[reservationKey];
            Validate.NotNull(reservation);

            if (reservation.CorrelationId != correlationId)
            {
                throw new UncorrelatedMessageException(); ;
            }

            Player player = new Player(reservation.PlayerId, connection);
            lock (playersByConnection)
            {
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
            lock (reservedPlayerNames)
            {
                reservedPlayerNames.Remove(playersByConnection[connection].Id);
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
