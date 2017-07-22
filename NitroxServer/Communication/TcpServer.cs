using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors;

namespace NitroxServer.Communication
{
    public class TcpServer
    {
        private Dictionary<String, Player> playersById;
        private Dictionary<Type, ServerPacketProcessor> packetProcessorsByType;
        private DefaultServerPacketProcessor defaultPacketProcessor;
        
        public TcpServer()
        {
            this.playersById = new Dictionary<String, Player>();
            this.defaultPacketProcessor = new DefaultServerPacketProcessor(this);
        }

        public void Start(Dictionary<Type, ServerPacketProcessor> packetProcessorsByType)
        {
            this.packetProcessorsByType = packetProcessorsByType;

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);            
            socket.Bind(localEndPoint);
            socket.Listen(4000);
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void ClientAccepted(IAsyncResult ar)
        {
            Console.WriteLine("New client connected");

            Socket socket = (Socket)ar.AsyncState;
            
            Connection connection = new Connection(socket.EndAccept(ar)); // TODO: Will this throw an error if timed correctly?
            connection.BeginReceive(new AsyncCallback(DataReceived));

            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }

        public void DataReceived(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            
            foreach(PlayerPacket packet in connection.GetPacketsFromRecievedData(ar))
            {
                Player player = GetPlayer(packet, connection);
                connection.PlayerId = player.Id;
                UpdatePlayerPosition(player, packet);

                if(packetProcessorsByType.ContainsKey(packet.GetType()))
                {
                    packetProcessorsByType[packet.GetType()].ProcessPacket(packet, player);
                }
                else
                {
                    defaultPacketProcessor.ProcessPacket(packet, player);
                }
            }

            if (connection.Open)
            {
                connection.BeginReceive(new AsyncCallback(DataReceived));
            }
            else
            {
                PlayerDisconnected(connection.PlayerId);
            }
        }

        private Player GetPlayer(PlayerPacket packet, Connection connection)
        {
            Player player;
            
            lock (playersById)
            {
                if (!playersById.TryGetValue(packet.PlayerId, out player))
                {
                    player = new Player(packet.PlayerId, connection);
                    playersById.Add(packet.PlayerId, player);
                }
            }
            
            return player;
        }

        private void UpdatePlayerPosition(Player player, PlayerPacket packet)
        {
            if (packet.GetType() == typeof(Movement))
            {
                player.Position = ((Movement)packet).PlayerPosition;
            }
        }

        private void SendCompleted(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;                
                int bytesSent = handler.EndSend(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Listener: Error sending packet");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void PlayerDisconnected(String PlayerId)
        {
            if (PlayerId != null)
            {
                lock (playersById)
                {
                    playersById.Remove(PlayerId);
                    PlayerPacket disconnectPacket = new Disconnect(PlayerId);
                    SendPacketToAllPlayersExcludingOne(disconnectPacket, PlayerId);
                    Console.WriteLine("Player disconnected: " + PlayerId);
                }
            }
        }

        public void SendPacketToAllPlayers(Packet packet)
        {
            lock (playersById)
            {
                foreach (Player player in playersById.Values)
                { 
                    if(player.Connection.Open)
                    {                         
                        player.Connection.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }                
                }
            }
        }

        public void SendPacketToAllPlayersExcludingOne(Packet packet, String excludeId)
        {
            lock (playersById)
            {
                foreach (Player player in playersById.Values)
                {
                    if (player.Id != excludeId && player.Connection.Open)
                    {
                        player.Connection.SendPacket(packet, new AsyncCallback(SendCompleted));
                    }
                }
            }
        }
    }
}
