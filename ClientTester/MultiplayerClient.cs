using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxModel.Logger;
using NitroxClient.Map;
using System;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public PacketSender PacketSender { get; private set; }
        public Logic Logic { get; private set; }
        public Vector3 clientPos = new Vector3(-50f, -2f, -38f);
        
        LoadedChunks loadedChunks;
        ChunkAwarePacketReceiver chunkAwarePacketReceiver;
        TcpClient client;

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
