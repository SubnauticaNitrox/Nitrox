using System;
using System.Collections.Generic;
using System.Threading;
using NitroxClient;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;

namespace ClientTester
{
    public class MultiplayerClient
    {
        private readonly string playerName;

        public Vector3 ClientPos = new Vector3(-50f, -2f, -38f);
        private DeferringPacketReceiver PacketReceiver { get; }
        private IMultiplayerSession MultiplayerSession { get; }

        public MultiplayerClient(ushort playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug | Log.LogLevel.FileLog);
            playerName = "Player" + playerId;

            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar());
            PacketReceiver = NitroxServiceLocator.LocateService<DeferringPacketReceiver>();
            MultiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
        }

        public void Start(string ip, int port)
        {
            Dictionary<Type, PacketProcessor> packetProcessorMap = GeneratePacketProcessorMap();
            MultiplayerSession.ConnectionStateChanged += ConnectionStateChangedHandler;
            MultiplayerSession.Connect(ip, port);

            for (int iterations = 0; iterations < 20; iterations++)
            {
                Thread.Sleep(250);
                ProcessPackets(packetProcessorMap);
            }
        }

        private Dictionary<Type, PacketProcessor> GeneratePacketProcessorMap()
        {
            Dictionary<Type, PacketProcessor> packetProcessorMap = new Dictionary<Type, PacketProcessor>
            {
                {typeof(MultiplayerSessionPolicy), new MultiplayerSessionPolicyProcessor(MultiplayerSession)},
                {typeof(MultiplayerSessionReservation), new MultiplayerSessionReservationProcessor(MultiplayerSession)}
            };

            return packetProcessorMap;
        }

        private void ConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.AwaitingReservationCredentials:
                    MultiplayerSession.RequestSessionReservation(new PlayerSettings(RandomColorGenerator.GenerateColor()), new AuthenticationContext(playerName));
                    break;
                case MultiplayerSessionConnectionStage.SessionReserved:
                    MultiplayerSession.JoinSession();
                    MultiplayerSession.ConnectionStateChanged -= ConnectionStateChangedHandler;
                    Log.InGame("SessionJoined to server");
                    break;
                case MultiplayerSessionConnectionStage.SessionReservationRejected:
                    Log.InGame("Unable to connect to server");
                    MultiplayerSession.ConnectionStateChanged -= ConnectionStateChangedHandler;
                    break;
            }
        }

        private void ProcessPackets(Dictionary<Type, PacketProcessor> packetProcessorMap)
        {
            Queue<Packet> packets = PacketReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                PacketProcessor packetProcessor;
                if (packetProcessorMap.TryGetValue(packet.GetType(), out packetProcessor))
                {
                    try
                    {
                        packetProcessor.ProcessPacket(packet, null);
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
