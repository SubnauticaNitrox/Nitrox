using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.Tcp;

namespace NitroxClient.Communication
{
    public class TcpClient
    {
        private ChunkAwarePacketReceiver packetReceiver;
        private const int port = 11000;
        private Connection connection;
        
        public TcpClient(ChunkAwarePacketReceiver packetManager)
        {
            this.packetReceiver = packetManager;
        }

        public void Start(String ip)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connection = new Connection(socket);

            socket.Connect(remoteEP);

            if(!socket.Connected)
            {
                throw new InvalidOperationException("Socket could not connect.");
            }

            connection.BeginReceive(new AsyncCallback(DataReceived));
        }

        public void Close()
        {
            connection.Close();
        }
        
        private void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;

            foreach (Packet packet in connection.GetPacketsFromRecievedData(ar))
            {
                packetReceiver.PacketReceived(packet);                
            }

            if (connection.Open)
            {
                connection.BeginReceive(new AsyncCallback(DataReceived));
            } else
            {
                Console.WriteLine("Disconnected from server.");
                // TODO: Disconnect gracefully, clean up
            }
        }

        public void Send(Packet packet)
        {
            connection.SendPacket(packet, new AsyncCallback(PacketSentSuccessful));
        }

        public void PacketSentSuccessful(IAsyncResult ar)
        {

        }
    }
}
