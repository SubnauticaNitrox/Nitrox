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
        private bool testClient = false;
        
        public TcpClient(ChunkAwarePacketReceiver packetManager)
        {
            this.packetReceiver = packetManager;
        }

        public TcpClient(ChunkAwarePacketReceiver packetManager, bool testClient)
        {
            this.packetReceiver = packetManager;
            this.testClient = testClient;
        }

        public void Start(String ip)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection = new Connection(socket);

                socket.Connect(remoteEP);

                if (!socket.Connected)
                {
                    OutputMessage("Unable to connect to server.");
                    throw new InvalidOperationException("Socket could not connect.");
                }
                else
                {
                    OutputMessage("Connected to server.");
                }

                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            catch (Exception)
            {
                OutputMessage("Unable to connect to server");
            }
        }

        public void Stop()
        {
            connection.Close(); // Server will clean up pretty quickly
            OutputMessage("Disconnected from server.");
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
                Console.WriteLine("Error reading data from server");
                Stop();
            }
        }

        public void Send(Packet packet)
        {
            connection.SendPacket(packet, new AsyncCallback(PacketSentSuccessful));
        }

        public void PacketSentSuccessful(IAsyncResult ar)
        {

        }

        public bool IsConnected()
        {
            if (connection == null)
            {
                return false;
            }
            return connection.Open;
        }

        private void OutputMessage(String msg)
        {
            if (testClient)
            {
                Console.WriteLine(msg);
            } else
            {
                ErrorMessage.AddMessage(msg);
            }
        }
    }
}
