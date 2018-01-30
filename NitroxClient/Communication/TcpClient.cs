using System;
using System.Net;
using System.Net.Sockets;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication
{
    public class TcpClient : IClient
    {
        private readonly DeferringPacketReceiver packetReceiver;
        private const int PORT = 11000;
        private Connection connection;

        public bool IsConnected { get; private set; }

        public TcpClient(DeferringPacketReceiver packetManager)
        {
            Log.Info("Initializing TcpClient...");
            packetReceiver = packetManager;
            IsConnected = false;
        }

        public void start(string ipAddress)
        {
            try
            {
                IPAddress address = IPAddress.Parse(ipAddress);
                IPEndPoint remoteEP = new IPEndPoint(address, PORT);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection = new Connection(socket);
                connection.Connect(remoteEP);

                if (connection.Open)
                {
                    IsConnected = true;
                    connection.BeginReceive(new AsyncCallback(DataReceived));
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unforeseen error when connecting: " + e.GetBaseException());
            }
        }

        public void stop()
        {
            connection.Close();
            IsConnected = false;
            Log.InGame("Disconnected from server.");
        }

        public void send(Packet packet)
        {
            connection.SendPacket(packet, new AsyncCallback(packetSentSuccessful));
        }

        public void packetSentSuccessful(IAsyncResult ar) { }

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
            }
            else
            {
                Log.Debug("Error reading data from server");
                stop();
            }
        }
    }
}
