using System;
using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxClient.Map;
using NitroxModel.Logger;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public PacketSender PacketSender { get; }
        public Logic Logic { get; }
        public Vector3 clientPos = new Vector3(-50f, -2f, -38f);

        private readonly LoadedChunks loadedChunks;
        private readonly ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        private readonly TcpClient client;

        public MultiplayerClient(String playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);
            loadedChunks = new LoadedChunks();
            chunkAwarePacketReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            client = new TcpClient(chunkAwarePacketReceiver);
            PacketSender = new PacketSender(client);
            PacketSender.PlayerId = playerId;
            Logic = new Logic(PacketSender, loadedChunks, chunkAwarePacketReceiver);
        }

        public void Start(String ip)
        {
            client.Start(ip);
            if (client.IsConnected())
            {
                Log.InGame("Connected to server");
                PacketSender.Active = true;
                PacketSender.Authenticate();
            }
            else
            {
                Log.InGame("Unable to connect to server");
            }
        }
    }
}
