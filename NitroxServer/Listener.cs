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
        private ConcurrentMap<String, Player> playersById = new ConcurrentMap<String, Player>();
        
        private HashSet<Type> packetForwardBlacklist;
        private HashSet<Type> loggingPacketBlackList;

        public Listener()
        {
            packetForwardBlacklist = new HashSet<Type>();
            packetForwardBlacklist.Add(typeof(Authenticate));

            loggingPacketBlackList = new HashSet<Type>();
            loggingPacketBlackList.Add(typeof(Movement));
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
            
            Connection connection = new Connection(socket.EndAccept(ar));
            connection.BeginReceive(new AsyncCallback(DataReceived));

            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            
            foreach(Packet packet in connection.GetPacketsFromRecievedData(ar))
            {
                Player player = getPlayer(packet, connection);
                connection.PlayerId = player.Id;
                UpdatePlayerPosition(player, packet);

                if (!loggingPacketBlackList.Contains(packet.GetType()))
                {
                    Console.WriteLine("Received packet from socket: " + packet.ToString() + " for player " + player.Id);
                }

                ForwardPacketToOtherPlayers(packet, player.Id);
            }

            if (connection.Open != false)
            {
                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            else
            {
                Packet disconnectPacket = new Disconnect(connection.PlayerId);
                ForwardPacketToOtherPlayers(disconnectPacket, connection.PlayerId);
                playersById.TryRemove(connection.PlayerId);
                Console.WriteLine("Player disconnected: " + connection.PlayerId);
            }
        }

        private Player getPlayer(Packet packet, Connection connection)
        {
            Player player;

            if(!playersById.TryGetValue(packet.PlayerId, out player))
            {
                player = new Player(packet.PlayerId, connection);
                playersById.TryAdd(packet.PlayerId, player);
            }
            
            return player;
        }

        private void UpdatePlayerPosition(Player player, Packet packet)
        {
            if (packet.GetType() == typeof(Movement))
            {
                player.Position = ((Movement)packet).PlayerPosition;
            }
        }
        
        private void ForwardPacketToOtherPlayers(Packet packet, String sendingPlayerId)
        {
            if (packetForwardBlacklist.Contains(packet.GetType()))
            {
                return;
            }

            foreach (Player player in playersById.values())
            {
                if (player.Id != sendingPlayerId && player.Connection.Open)
                {
                    player.Connection.SendPacket(packet, new AsyncCallback(SendCompleted));
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
