using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic;
using NitroxClient.Map;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        public IPacketSender PacketSender { get; }
        public MultiplayerSessionManager MultiplayerSessionManager { get; }
        public Logic Logic { get; }
        public object PacketProcessorsByType { get; private set; }

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
            MultiplayerSessionManager = new MultiplayerSessionManager(client);
            PacketSender = MultiplayerSessionManager;
            Logic = new Logic(MultiplayerSessionManager, visibleCells, packetReceiver);
        }

        public void Start(string ip)
        {
            MultiplayerSessionManager.Connect(ip, playerName);

            var iterations = 0;
            do
            {
                Thread.Sleep(250);
                ProcessPackets();

                iterations++;
                if (iterations >= 20)
                {
                    break;
                }
            } while (MultiplayerSessionManager.CurrentState == ClientBridgeState.WaitingForRerservation);

            switch (MultiplayerSessionManager.CurrentState)
            {
                case ClientBridgeState.Reserved:
                    MultiplayerSessionManager.ClaimReservation();
                    Log.InGame("Connected to server");
                    break;
                default:
                    Log.InGame("Unable to connect to server");
                    break;
            }
        }

        private void ProcessPackets()
        {
            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                if (packet.GetType() == typeof(PlayerSlotReservation))
                {
                    try
                    {
                        PacketProcessor processor = new PlayerSlotReservationProcessor(MultiplayerSessionManager);
                        processor.ProcessPacket(packet, null);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error processing packet: " + packet, ex);
                    }
                }
                else
                {
                    Log.Debug("No packet processor for the given type: " + packet.GetType());
                }
            }
        }
    }
}
