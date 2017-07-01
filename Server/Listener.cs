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
                String playerId = packet.PlayerId;

                Player player;
                playersById.TryGetValue(playerId, out player);

                if (player == null)
                {
                    player = new Player(playerId, connection);
                    playersById.TryAdd(playerId, player);
                }

                if(packet.GetType() == typeof(Movement))
                {
                    player.Position = ((Movement)packet).PlayerPosition;
                }

                if (!loggingPacketBlackList.Contains(packet.GetType()))
                {
                    Console.WriteLine("Received packet from socket: " + packet.ToString() + "for player " + playerId);
                }

                ForwardPacketToOtherPlayers(packet, playerId);
            }

            connection.BeginReceive(new AsyncCallback(DataReceived));
        }

        private void ForwardPacketToOtherPlayers(Packet packet, String sendingPlayerId)
        {
            if (packetForwardBlacklist.Contains(packet.GetType()))
            {
                return;
            }

            foreach (Player player in playersById.values())
            {
                if (player.Id != sendingPlayerId)
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
