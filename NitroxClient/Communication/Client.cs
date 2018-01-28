using System;
using System.Net;
using System.Net.Sockets;
using NitroxClient.MonoBehaviours;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Tcp;

namespace NitroxClient.Communication
{
    public class TcpClient
    {
        private readonly DeferringPacketReceiver packetReceiver;
        private const int PORT = 11000;
        private Connection connection;

        public TcpClient(DeferringPacketReceiver packetManager)
        {
            packetReceiver = packetManager;
        }

        public void Start(string ip)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection = new Connection(socket);
                connection.Connect(remoteEP);

                if (connection.Open)
                {
                    connection.BeginReceive(new AsyncCallback(DataReceived));
                }
            }
            catch (Exception e)
            {
                Log.Debug("Unforeseen error when connecting: " + e.GetBaseException());
            }
        }

        public void Stop()
        {
            connection.Close(); // Server will clean up pretty quickly
            Multiplayer.Logic.PacketSender.Active = false;
            Multiplayer.RemoveAllOtherPlayers();
            Log.InGame("Disconnected from server.");
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
            }
            else
            {
                Log.Debug("Error reading data from server");
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
