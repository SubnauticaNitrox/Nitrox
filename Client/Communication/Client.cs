using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    class TcpClient
    {
        private const int port = 11000;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static String response = String.Empty;
        private Connection connection;
        
        public Dictionary<Type, Queue<Packet>> ReceivedPacketsByType { get; protected set; }

        public void Start()
        {
            ReceivedPacketsByType = new Dictionary<Type, Queue<Packet>>();
            
            IPHostEntry ipHostInfo = Dns.Resolve("104.232.113.100");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            connection = new Connection();
            connection.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Socket socket = (Socket)connection.Socket;

            socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), connection);
            connectDone.WaitOne();

            Receive(connection);
        }

        public void Stop()
        {
            Socket socket = (Socket)connection.Socket;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            Socket socket = (Socket)connection.Socket;

            Console.WriteLine("Socket connected to {0}",
                socket.RemoteEndPoint.ToString());

            connectDone.Set();
        }

        private void Receive(Connection connection)
        {
            Socket socket = (Socket)connection.Socket;

            socket.BeginReceive(connection.MessagePieceBuffer, 0, Connection.MessagePieceBufferSize, 0,
                new AsyncCallback(PacketPieceReceived), connection);   
        }

        private void PacketPieceReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            Socket client = (Socket)connection.Socket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                lock (connection)
                {
                    connection.ProcessNewMessagePiecesInBuffer(bytesRead);
                }

                while (connection.ReceivedPackets.Count > 0)
                {
                    Packet incomingPacket = connection.ReceivedPackets.Dequeue();

                    lock (ReceivedPacketsByType)
                    {
                        if (!ReceivedPacketsByType.ContainsKey(incomingPacket.GetType()))
                        {
                            ReceivedPacketsByType[incomingPacket.GetType()] = new Queue<Packet>();
                        }

                        ReceivedPacketsByType[incomingPacket.GetType()].Enqueue(incomingPacket);
                    }
                }
            }

            client.BeginReceive(connection.MessagePieceBuffer, 0, Connection.MessagePieceBufferSize, 0, new AsyncCallback(PacketPieceReceived), connection);
        }

        public Queue<T> getReceivedPacketsOfType<T>() where T : Packet
        {
            Queue<T> packetsOfType = new Queue<T>();

            lock (ReceivedPacketsByType)
            {
                if (ReceivedPacketsByType.ContainsKey(typeof(T)))
                {
                    while (ReceivedPacketsByType[typeof(T)].Count > 0)
                    {
                        packetsOfType.Enqueue((T)ReceivedPacketsByType[typeof(T)].Dequeue());
                    }
                }
            }

            return packetsOfType;
        }

        public void Send(Packet packet)
        {
            byte[] packetData;
            BinaryFormatter bf = new BinaryFormatter();

            //DataContractJsonSerializer formatter = new DataContractJsonSerializer(packet.GetType());

            Socket socket = (Socket)connection.Socket;

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, packet);
                packetData = ms.ToArray();
            }

            lock (connection.Socket)
            {
                using (var stream = new NetworkStream(socket))
                {
                    Int16 packetSize = (Int16)packetData.Length;
                    byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

                    stream.Write(packetSizeBytes, 0, packetSizeBytes.Length);
                    stream.Write(packetData, 0, packetData.Length);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            int bytesSent = client.EndSend(ar);
        }
    }
}
