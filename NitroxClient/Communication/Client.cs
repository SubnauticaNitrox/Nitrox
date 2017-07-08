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
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection = new Connection(socket);

                socket.Connect(remoteEP);

                if (!socket.Connected)
                {
                    ErrorMessage.AddMessage("Unable to connect to server.");
                    throw new InvalidOperationException("Socket could not connect.");
                }
                else
                {
                    ErrorMessage.AddMessage("Connected to server.");
                }

                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to server");
                ErrorMessage.AddMessage("Unable to connect to server");
            }
        }

        public void Stop()
        {
            connection.Close(); // Server will clean up pretty quickly
            ErrorMessage.AddMessage("Disconnected from server.");
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

        public bool isConnected()
        {
            if (connection == null)
            {
                return false;
            }
            return connection.Open;
        }
    }
}
