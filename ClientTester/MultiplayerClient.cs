using NitroxClient;
using NitroxClient.Communication;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxClient.Map;
using System;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public PacketSender PacketSender { get; set; }

        MultiplayerObjectManager multiplayerObjectManager;
        LoadedChunks loadedChunks;
        ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        TcpClient client;

        public MultiplayerClient(String playerId)
        {
            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client, multiplayerObjectManager);
            PacketSender.PlayerId = playerId;
        }

        public void Start(String ip)
        {
            client.Start(ip);
            PacketSender.Active = true;
            PacketSender.Authenticate();
        }
    }
}
