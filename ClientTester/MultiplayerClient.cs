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
        
        VisibleCells visibleCells;
        DeferringPacketReceiver packetReceiver;
        TcpClient client;

        public MultiplayerClient(String playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);
            visibleCells = new VisibleCells();
            packetReceiver = new DeferringPacketReceiver(visibleCells);
            client = new TcpClient(packetReceiver);
            PacketSender = new PacketSender(client);
            PacketSender.PlayerId = playerId;
            Logic = new Logic(PacketSender, visibleCells, packetReceiver);
        }

        public void Start(String ip)
        {
            client.Start(ip);
            if (client.IsConnected())
            {
                Log.InGame("Connected to server");
                PacketSender.Active = true;
                Logic.Player.Authenticate(PacketSender.PlayerId);
            }
            else
            {
                Log.InGame("Unable to connect to server");
            }
        }
    }
}
