using System;
using System.Net;
using System.Net.Sockets;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Tcp;

namespace NitroxClient.Communication
{
    public class TcpClient : IClient
    {
        private readonly DeferringPacketReceiver packetReceiver;
        private int port = 11000;
        private Connection connection;

        public bool IsConnected { get; private set; }

        public TcpClient(DeferringPacketReceiver packetManager)
        {
            Log.Info("Initializing TcpClient...");
            packetReceiver = packetManager;
            IsConnected = false;
        }

        public void Start(string ipAddress)
        {
            try
            {
                //If Ip address includes a port use the included port.
                Validate.IsTrue(!string.IsNullOrEmpty(ipAddress));
                string[] splitIp = ipAddress.Split(':');
                ipAddress = splitIp[0];
                if (splitIp.Length > 1)
                {
                    port = int.Parse(splitIp[1]);
                    if (splitIp.Length > 2)
                    {
                        throw new Exception("Multiple Ports Detected!");
                    }
                }

                IPAddress address = IPAddress.Parse(ipAddress);
                Log.Info(ipAddress);
                
                IPEndPoint remoteEP = new IPEndPoint(address, port);
                Log.Info(port);
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

        public void Stop()
        {
            connection.Close();
            IsConnected = false;
            Log.InGame("Disconnected from server.");
        }

        public void Send(Packet packet)
        {
            connection.SendPacket(packet, new AsyncCallback(PacketSentSuccessful));
        }

        public void PacketSentSuccessful(IAsyncResult ar) { }

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
    }
}
