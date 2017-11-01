using NitroxModel.Packets;
using NitroxModel.Tcp;
using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class PlayerManager
    {
        private readonly Dictionary<Connection, Player> playersByConnection = new Dictionary<Connection, Player>();
        
        public Player PlayerAuthenticated(Connection connection, string playerId)
        {
            Player player = new Player(playerId, connection);

            lock (playersByConnection)
            {
                playersByConnection[connection] = player;
            }

            return player;
        }

        public void PlayerDisconnected(Connection connection)
        {
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
