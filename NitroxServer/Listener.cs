using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.Tcp;

namespace NitroxServer
{
    public class Listener
    {
        public event EventHandler PlayerAuthenticated;

        private Dictionary<String, Player> playersById = new Dictionary<String, Player>();

        private HashSet<Type> packetForwardBlacklist;
        private HashSet<Type> loggingPacketBlackList;

        public Listener()
        {
            packetForwardBlacklist = new HashSet<Type>
            {
                typeof(Authenticate)
            };

            loggingPacketBlackList = new HashSet<Type>
            {
                typeof(AnimationChangeEvent),
                typeof(Movement),
                typeof(VehicleMovement),
                typeof(ItemPosition)
            };
        }

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(4000);
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void ClientAccepted(IAsyncResult ar)
        {
            Console.WriteLine("New client connected");

            Socket socket = (Socket)ar.AsyncState;

            Connection connection = new Connection(socket.EndAccept(ar)); // TODO: Will this throw an error if timed correctly?
            connection.BeginReceive(new AsyncCallback(DataReceived));

            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;

            foreach (PlayerPacket packet in connection.GetPacketsFromRecievedData(ar))
            {
                if (connection.Authenticated)
                {
                    ParsePacket(connection, packet);
                }
                else
                {
                    if (packet is Authenticate)
                    {
                        Player player = GetPlayer(packet, connection);
                        connection.PlayerId = player.Id;
                        Console.WriteLine("Player authenticated: " + player.Id);
                        PlayerPacket connectPacket = new Connect(packet.PlayerId);
                        ForwardPacketToOtherPlayers(connectPacket, packet.PlayerId);
                        connection.Authenticated = true;
                    }
                    else
                    {
                        Console.WriteLine("Dropping packet (unauthenticated): " + packet.ToString());
                    }
                }
            }
            if (connection.Open)
            {
                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            else
            {
                PlayerDisconnected(connection.PlayerId);
            }
        }

        private void ParsePacket(Connection connection, PlayerPacket packet)
        {
            Player player = GetPlayer(packet, connection);
            connection.PlayerId = player.Id;
            UpdatePlayerPosition(player, packet);

            if (!loggingPacketBlackList.Contains(packet.GetType()))
            {
                Console.WriteLine("Received packet from socket: " + packet.ToString() + " for player " + player.Id);
            }

            ForwardPacketToOtherPlayers(packet, player.Id);
        }

        private Player GetPlayer(PlayerPacket packet, Connection connection)
        {
            Player player;
            
            lock (playersById)
            {
                if (!playersById.TryGetValue(packet.PlayerId, out player))
                {
                    player = new Player(packet.PlayerId, connection);
                    playersById.Add(packet.PlayerId, player);
                    PlayerAuthenticated(this, EventArgs.Empty);
                }
            }

            return player;
        }

        private void UpdatePlayerPosition(Player player, PlayerPacket packet)
        {
            if (packet.GetType() == typeof(Movement))
            {
                player.Position = ((Movement)packet).PlayerPosition;
            }
        }

        private void ForwardPacketToOtherPlayers(PlayerPacket packet, String sendingPlayerId)
        {
            if (packetForwardBlacklist.Contains(packet.GetType()))
            {
                return;
            }

            lock (playersById)
            {
                foreach (Player player in playersById.Values)
                {
                    if (player.Id != sendingPlayerId && player.Connection.Open)
                    {
                        player.Connection.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }
                }
            }
        }

        private void SendCompleted(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Listener: Error sending packet");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void PlayerDisconnected(String PlayerId)
        {
            if (PlayerId != null)
            {
                lock (playersById)
                {
                    playersById.Remove(PlayerId);
                    PlayerPacket disconnectPacket = new Disconnect(PlayerId);
                    ForwardPacketToOtherPlayers(disconnectPacket, PlayerId);
                    Console.WriteLine("Player disconnected: " + PlayerId);
                }
            }
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            lock (playersById)
            {
                foreach (Player player in playersById.Values)
                {
                    player.Connection.SendPacket(packet, new AsyncCallback(SendCompleted));                    
                }
            }
        }
    }
}
