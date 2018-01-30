using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxClient.Map;
using NitroxModel.Logger;
using System.Threading;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public IPacketSender PacketSender { get; }
        public ClientBridge ClientBridge { get; }
        public Logic Logic { get; }
        public Vector3 ClientPos = new Vector3(-50f, -2f, -38f);

        private readonly VisibleCells visibleCells;
        private readonly DeferringPacketReceiver packetReceiver;
        private readonly TcpClient client;
        private readonly string playerName;

        public MultiplayerClient(string playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);
            playerName = playerId;
            visibleCells = new VisibleCells();
            packetReceiver = new DeferringPacketReceiver(visibleCells);
            client = new TcpClient(packetReceiver);
            ClientBridge = new ClientBridge(client);
            PacketSender = ClientBridge;
            Logic = new Logic(ClientBridge, visibleCells, packetReceiver);
        }

        public void Start(string ip)
        {
            ClientBridge.Connect(ip, playerName);

            var iterations = 0;
            while(ClientBridge.CurrentState == ClientBridgeState.WaitingForRerservation)
            {
                Thread.Sleep(250);

                iterations++;
                if(iterations >= 20)
                {
                    break;
                }
            }

            switch (ClientBridge.CurrentState)
            {
                case ClientBridgeState.Reserved:
                    ClientBridge.ClaimReservation();
                    Log.InGame("Connected to server");
                    break;
                default:
                    Log.InGame("Unable to connect to server");
                    break;
            }
        }
    }
}
