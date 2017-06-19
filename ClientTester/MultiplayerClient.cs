using NitroxClient;
using NitroxClient.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
