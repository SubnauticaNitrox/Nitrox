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
        public Vector3 ClientPos = new Vector3(-50f, -2f, -38f);

        private readonly VisibleCells visibleCells;
        private readonly DeferringPacketReceiver packetReceiver;
        private readonly TcpClient client;

        public MultiplayerClient(string playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);
            visibleCells = new VisibleCells();
            packetReceiver = new DeferringPacketReceiver(visibleCells);
            client = new TcpClient(packetReceiver);
            PacketSender = new PacketSender(client, playerId);
            Logic = new Logic(PacketSender, visibleCells, packetReceiver);
        }

        public void Start(string ip)
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
