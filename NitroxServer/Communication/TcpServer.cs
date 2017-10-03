using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxServer.Communication
{
    public class TcpServer
    {
        private PacketHandler packetHandler;
        private Dictionary<Player, Connection> connectionsByPlayer;

        public TcpServer()
        {
            this.connectionsByPlayer = new Dictionary<Player, Connection>();
        }

        public void Start(PacketHandler packetHandler)
        {
            this.packetHandler = packetHandler;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(4000);
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void ClientAccepted(IAsyncResult ar)
        {
            Log.Info("New client connected");

            Socket socket = (Socket)ar.AsyncState;

            PlayerConnection connection = new PlayerConnection(socket.EndAccept(ar)); // TODO: Will this throw an error if timed correctly?
            connection.BeginReceive(new AsyncCallback(DataReceived));

            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void DataReceived(IAsyncResult ar)
        {
            PlayerConnection connection = (PlayerConnection)ar.AsyncState;

            foreach (Packet packet in connection.GetPacketsFromRecievedData(ar))
            {
                try
                {
                    if (connection.Player == null)
                    {
                        packetHandler.ProcessUnauthenticated(packet, connection);
                    }
                    else
                    {
                        packetHandler.ProcessAuthenticated(packet, connection.Player);
                    }
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

        private void SendCompleted(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Log.Info("Error sending packet: " + e.ToString());
            }
        }

        private void PlayerDisconnected(PlayerConnection connection)
        {
            Player player = connection.Player;

            if (player != null)
            {
                lock (connectionsByPlayer)
                {
                    connectionsByPlayer.Remove(player);
                }

                Disconnect disconnectPacket = new Disconnect(player.Id);
                SendPacketToAllPlayers(disconnectPacket);
                Log.Info("Player disconnected: " + player.Id);
            }
        }

        public void SendPacketToPlayer(Packet packet, Player player)
        {
            Connection connection;

            lock (connectionsByPlayer)
            {
                connection = connectionsByPlayer[player];
            }

            SendPacketToConnection(packet, connection);
        }

        public void SendPacketToConnection(Packet packet, Connection connection)
        {
            if (connection.Open)
            {
                connection.SendPacket(packet, new AsyncCallback(SendCompleted));
            }
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            lock (connectionsByPlayer)
            {
                foreach (Connection connection in connectionsByPlayer.Values)
                {
                    if (connection.Open)
                    {
                        connection.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }
                }
            }
        }

        public void SendPacketToOtherPlayers(Packet packet, Player sendingPlayer)
        {
            lock (connectionsByPlayer)
            {
                foreach (KeyValuePair<Player, Connection> connectWithPlayer in connectionsByPlayer)
                {
                    if (connectWithPlayer.Key != sendingPlayer && connectWithPlayer.Value.Open)
                    {
                        connectWithPlayer.Value.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }
                }
            }
        }

        public void SendPacketToPlayersInChunk(Packet packet, Chunk chunk)
        {
            lock (connectionsByPlayer)
            {
                foreach (KeyValuePair<Player, Connection> connectWithPlayer in connectionsByPlayer)
                {
                    if(connectWithPlayer.Key.HasChunkLoaded(chunk))
                    {
                        connectWithPlayer.Value.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }
                }
            }
        }

        public void PlayerAuthenticated(Player player, PlayerConnection connection)
        {
            connection.Player = player;

            lock (connectionsByPlayer)
            {
                connectionsByPlayer[player] = connection;
            }
        }
    }
}
