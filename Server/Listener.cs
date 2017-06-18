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

namespace NitroxServer
{
    public class Listener
    {
        private ManualResetEvent connectionEstablished = new ManualResetEvent(false);

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

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(4000);

                while (true)
                {
                    connectionEstablished.Reset();
                    
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(ClientAccepted), listener);

                    connectionEstablished.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadLine();
        }

        public void ClientAccepted(IAsyncResult ar)
        {
            connectionEstablished.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            
            Connection connection = new Connection();
            connection.Socket = handler;

            handler.BeginReceive(connection.MessagePieceBuffer, 0, Connection.MessagePieceBufferSize, 0, new AsyncCallback(PacketPieceReceived), connection);
        }

        public void PacketPieceReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            Socket handler = connection.Socket;
            
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                lock (connection)
                {
                    connection.ProcessNewMessagePiecesInBuffer(bytesRead);
                }

                while(connection.ReceivedPackets.Count > 0) 
                {
                    Packet incomingPacket = connection.ReceivedPackets.Dequeue();
                    String playerId = incomingPacket.PlayerId;

                    Player player;
                    playersById.TryGetValue(playerId, out player);

                    if (player == null)
                    {
                        player = new Player(playerId, connection);
                        playersById.TryAdd(playerId, player);
                    }

                    if(incomingPacket.GetType() == typeof(Movement))
                    {
                        player.Position = ((Movement)incomingPacket).PlayerPosition;
                    }

                    if (!loggingPacketBlackList.Contains(incomingPacket.GetType()))
                    {
                        Console.WriteLine("Received packet from socket: " + incomingPacket.ToString() + "for player " + playerId);
                    }

                    ForwardPacketToOtherPlayers(incomingPacket, playerId);
                }
            }

            handler.BeginReceive(connection.MessagePieceBuffer, 0, Connection.MessagePieceBufferSize, 0, new AsyncCallback(PacketPieceReceived), connection);
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
                    Send(player.Connection, packet);                    
                }
            }            
        }
        
        private void Send(Connection connection, Packet packet)
        {
            byte[] packetData;
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                //place holder for size, will be filled in later... allows us
                //to avoid doing a byte array merge... zomg premature optimization
                ms.Write(new Byte[] { 0x00, 0x00 }, 0, 2);
                bf.Serialize(ms, packet);
                packetData = ms.ToArray();
            }

            Int16 packetSize = (Int16)(packetData.Length - 2); // subtract 2 because we dont want to take into account the added bytes
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            //premature optimization continued :)
            packetData[0] = packetSizeBytes[0];
            packetData[1] = packetSizeBytes[1];

            connection.Socket.BeginSend(packetData, 0, packetData.Length, 0, new AsyncCallback(SendCompleted), connection.Socket);
        }

        private void SendCompleted(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
