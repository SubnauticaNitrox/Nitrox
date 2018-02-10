using System;
using System.Net;
using System.Net.Sockets;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxModel.Tcp;

namespace NitroxServer.Communication
{
    public class TcpServer
    {
        private readonly PacketHandler packetHandler;
        private readonly PlayerManager playerManager;

        public TcpServer(PacketHandler packetHandler, PlayerManager playerManager)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
        }

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(4000);
            socket.BeginAccept(ClientAccepted, socket);
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            Log.Info("New client connected");

            Socket socket = (Socket)ar.AsyncState;

            Connection connection = new Connection(socket.EndAccept(ar)); // TODO: Will this throw an error if timed correctly?
            connection.BeginReceive(DataReceived);

            socket.BeginAccept(ClientAccepted, socket);
        }

        private void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;

            foreach (Packet packet in connection.GetPacketsFromRecievedData(ar))
            {
                try
                {
                    packetHandler.Process(packet, connection);
                }
                catch (Exception ex)
                {
                    Log.Info("Exception while processing packet: " + packet + " " + ex);
                }
            }

            if (connection.Open)
            {
                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            else
            {
                PlayerDisconnected(connection);
            }
        }
        
        private void PlayerDisconnected(Connection connection)
        {
            Player player = playerManager.GetPlayer(connection);

            if(player != null)
            {
                playerManager.PlayerDisconnected(connection);

                Disconnect disconnect = new Disconnect(player.Id);
                playerManager.SendPacketToAllPlayers(disconnect);
            }
        }     
    }
}
