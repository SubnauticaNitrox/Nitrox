using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxClient.Logger;
using NitroxClient.MonoBehaviours;
using System;
using System.Net;
using System.Net.Sockets;

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
                    ClientLogger.WriteLine("Unable to connect to server.");
                    throw new InvalidOperationException("Socket could not connect.");
                }
                else
                {
                    ClientLogger.WriteLine("Connected to server.");
                }

                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            catch (Exception)
            {
                ClientLogger.WriteLine("Unforeseen error when connecting: " + e.GetBaseException());
                throw new InvalidOperationException("Unknown error while connecting");
            }
        }

        public void Stop()
        {
            connection.Close(); // Server will clean up pretty quickly
            Multiplayer.PacketSender.Active = false;
            Multiplayer.RemoveAllOtherPlayers();
            ClientLogger.WriteLine("Disconnected from server.");
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
                ClientLogger.DebugLine("Error reading data from server");
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
    }
}
