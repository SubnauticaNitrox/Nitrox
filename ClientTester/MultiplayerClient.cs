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
        private DeferringPacketReceiver packetReceiver { get; }
        private IMultiplayerSession multiplayerSession { get; }

        public Vector3 ClientPos = new Vector3(-50f, -2f, -38f);

        private readonly string playerName;

        public MultiplayerClient(ulong playerId)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);
            playerName = "Player" + playerId.ToString();

            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFaqRegistrar());
            packetReceiver = NitroxServiceLocator.LocateService<DeferringPacketReceiver>();
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
        }

        public void Start(string ip)
        {
            Dictionary<Type, PacketProcessor> packetProcessorMap = GeneratePacketProcessorMap();
            multiplayerSession.ConnectionStateChanged += ConnectionStateChangedHandler;
            multiplayerSession.Connect(ip);

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
                { typeof(MultiplayerSessionPolicy), new MultiplayerSessionPolicyProcessor(multiplayerSession) },
                { typeof(MultiplayerSessionReservation), new MultiplayerSessionReservationProcessor(multiplayerSession) }
            };

            return packetProcessorMap;
        }

        private void ConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.AwaitingReservationCredentials:
                    multiplayerSession.RequestSessionReservation(new PlayerSettings(RandomColorGenerator.GenerateColor()), new AuthenticationContext(playerName, Steamworks.SteamUser.GetSteamID().m_SteamID));
                    break;
                case MultiplayerSessionConnectionStage.SessionReserved:
                    multiplayerSession.JoinSession();
                    multiplayerSession.ConnectionStateChanged -= ConnectionStateChangedHandler;
                    Log.InGame("SessionJoined to server");
                    break;
                case MultiplayerSessionConnectionStage.SessionReservationRejected:
                    Log.InGame("Unable to connect to server");
                    multiplayerSession.ConnectionStateChanged -= ConnectionStateChangedHandler;
                    break;
            }
        }

        private void ProcessPackets(Dictionary<Type, PacketProcessor> packetProcessorMap)
        {
            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

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
