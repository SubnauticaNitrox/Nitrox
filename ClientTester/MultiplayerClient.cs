using NitroxClient;
using NitroxClient.Communication;
using NitroxClient.Map;
using System;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public PacketSender PacketSender { get; set; }

        LoadedChunks loadedChunks;
        ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        TcpClient client;

        public MultiplayerClient(String playerId)
        {
            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver, true);
            PacketSender = new PacketSender(client);
            PacketSender.PlayerId = playerId;
        }

        public void Start(String ip)
        {
            client.Start(ip);
            if (client.isConnected())
            {
                PacketSender.Active = true;
                PacketSender.Authenticate();
            }
        }
    }
}
