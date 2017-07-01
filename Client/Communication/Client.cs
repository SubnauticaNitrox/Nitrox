using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Tcp;

namespace NitroxClient.Communication
{
    public class TcpClient
    {
        private ChunkAwarePacketReceiver packetReceiver;
        private const int port = 11000;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static String response = String.Empty;
        private Connection connection;
        
        public TcpClient(ChunkAwarePacketReceiver packetManager)
        {
            this.packetReceiver = packetManager;
        }

        public void Start()
        {
            IPHostEntry ipHostInfo = Dns.Resolve("104.232.113.100");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connection = new Connection(socket);

            socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), connection);
            connectDone.WaitOne();

            connection.BeginReceive(new AsyncCallback(DataReceived));
        }

        public void Close()
        {
            connection.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            connectDone.Set();
        }
        
        private void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;

            foreach (Packet packet in connection.GetPacketsFromRecievedData(ar))
            {
                packetReceiver.PacketReceived(packet);                
            }

            connection.BeginReceive(new AsyncCallback(DataReceived));
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
