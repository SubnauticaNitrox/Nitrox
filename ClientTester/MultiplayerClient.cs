using NitroxClient;
using NitroxClient.Communication;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxClient.Map;
using System;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public PacketSender PacketSender { get; set; }
        public Vector3 clientPos = new Vector3(-50f, -2f, -38f);

        MultiplayerObjectManager multiplayerObjectManager;
        LoadedChunks loadedChunks;
        ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        TcpClient client;

        public MultiplayerClient(String playerId)
        {
            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver, true);
            PacketSender = new PacketSender(client, multiplayerObjectManager);
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
