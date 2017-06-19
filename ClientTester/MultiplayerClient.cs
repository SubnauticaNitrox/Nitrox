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
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client, playerId);
        }

        public void Start()
        {
            client.Start();
            PacketSender.Authenticate();
        } 

    }
}
