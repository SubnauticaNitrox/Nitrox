using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxClient.Map;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxReloader;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    //This class is getting really big and is taking on many responsibilities. It might be worth a joint effort to see if we can plan some refactoring to this guy at some point in the future.
    public class Multiplayer : MonoBehaviour
    {
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";

        public static Multiplayer Main;

        public static event Action OnBeforeMultiplayerStart;

        private static readonly VisibleCells visibleCells = new VisibleCells();
        private static readonly DeferringPacketReceiver packetReceiver = new DeferringPacketReceiver(visibleCells);
        private static readonly TcpClient client = new TcpClient(packetReceiver);
        private static readonly ClientBridge clientBridge = new ClientBridge(client);

        //One ring, to rule them all...
        public static readonly Logic Logic = new Logic(clientBridge, visibleCells, packetReceiver);

        private static bool hasLoadedMonoBehaviors;

        private static readonly PlayerManager remotePlayerManager = new PlayerManager();
        private static readonly PlayerVitalsManager remotePlayerVitalsManager = new PlayerVitalsManager();
        private static readonly PlayerChatManager remotePlayerChatManager = new PlayerChatManager();

        public static Dictionary<Type, PacketProcessor> PacketProcessorsByType;

        // List of arguments that can be used in a processor:
        private static Dictionary<Type, object> processorArguments = new Dictionary<Type, object>
        {
            { typeof(PlayerManager), remotePlayerManager },
            { typeof(PlayerVitalsManager), remotePlayerVitalsManager },
            { typeof(PlayerChatManager), remotePlayerChatManager },
            { typeof(IPacketSender), clientBridge },
            { typeof(ClientBridge), clientBridge }
        };

        static Multiplayer()
        {
            Log.Info("Initializing Multiplayer Client...");
            PacketProcessorsByType = PacketProcessor.GetProcessors(processorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>));
            Log.Info("Multiplayer Client Initialized...");
        }

        public void Awake()
        {
            Log.Info("Multiplayer Waking Up...");
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);

            Main = this;
        }

        public void Update()
        {
            Reloader.ReloadAssemblies();
            if (clientBridge.CurrentState != ClientBridgeState.Disconnected && 
                clientBridge.CurrentState != ClientBridgeState.Failed)
            {
                ProcessPackets();
            }
        }

        public void ProcessPackets()
        {
            Queue<Packet> packets = packetReceiver.GetReceivedPackets();            

            foreach (Packet packet in packets)
            {
                if (PacketProcessorsByType.ContainsKey(packet.GetType()))
                {
                    try
                    {
                        PacketProcessor processor = PacketProcessorsByType[packet.GetType()];
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

        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            if (clientBridge.CurrentState == ClientBridgeState.Connected)
            {
                Log.InGame("Already connected to a server");
            }
            else if (n?.data?.Count > 0)
            {
                NegotiatePlayerSlotReservation(n.data.Count >= 2 ? (string)n.data[1] : DEFAULT_IP_ADDRESS, (string)n.data[0]);
                StartCoroutine(HandleReservationFromConsole());
            }
            else
            {
                Log.InGame("Command syntax: mplayer USERNAME [SERVERIP]");
            }
        }

        public void OnConsoleCommand_disconnect(NotificationCenter.Notification n)
        {
            if (n != null)
            {
                StopMultiplayer(); // TODO: More than just disconnect (clean up injections or something)
            }
        }

        public void OnConsoleCommand_warpto(NotificationCenter.Notification n)
        {
            if (n?.data?.Count > 0)
            {
                string otherPlayerId = (string)n.data[0];
                Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(otherPlayerId);
                if (opPlayer.IsPresent())
                {
                    Player.main.SetPosition(opPlayer.Get().Body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        public void NegotiatePlayerSlotReservation(string ipAddress, string playerName)
        {
            clientBridge.connect(ipAddress, playerName);
        }

        public void JoinSession()
        {
            OnBeforeMultiplayerStart();
            clientBridge.claimReservation();
            InitMonoBehaviours();
        }

        public void InitMonoBehaviours()
        {
            if (!hasLoadedMonoBehaviors)
            {
                gameObject.AddComponent<Chat>();
                gameObject.AddComponent<PlayerMovement>();
                gameObject.AddComponent<PlayerStatsBroadcaster>();
                gameObject.AddComponent<AnimationSender>();
                gameObject.AddComponent<EntityPositionBroadcaster>();

                hasLoadedMonoBehaviors = true;
            }
        }

        public void ForceActivation()
        {
            gameObject.SetActive(true);
        }

        private IEnumerator HandleReservationFromConsole()
        {
            yield return new WaitUntil(() => clientBridge.CurrentState != ClientBridgeState.WaitingForRerservation);

            switch (clientBridge.CurrentState)
            {
                case ClientBridgeState.Reserved:
                    JoinSession();
                    break;
                case ClientBridgeState.ReservationRejected:
                    Log.InGame($"Cannot join server: {clientBridge.ReservationRejectionReason.ToString()}");
                    break;
                default:
                    Log.InGame("Unable to communicate with the server for unknown reasons.");
                    break;
            }
        }

        private void StopMultiplayer()
        {
            remotePlayerManager.RemoveAllPlayers();
            clientBridge.disconnect();
        }
    }
}
