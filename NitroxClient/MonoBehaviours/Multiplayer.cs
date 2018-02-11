using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole;
using NitroxModel.NitroxConsole.Events;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxReloader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
{
    //This class is getting really big and is taking on many responsibilities. It might be worth a joint effort to see if we can plan some refactoring to this guy at some point in the future.
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main { get; private set; }
        public static bool IsLoading { get; private set; }
        private GameObject synchronizers;

        private static readonly VisibleCells visibleCells = new VisibleCells();
        private static readonly DeferringPacketReceiver packetReceiver = new DeferringPacketReceiver(visibleCells);
        private static readonly TcpClient client = new TcpClient(packetReceiver);
        private static readonly ClientBridge clientBridge = new ClientBridge(client);

        //One ring, to rule them all...
        public static readonly Logic Logic = new Logic(clientBridge, visibleCells, packetReceiver);

        private static readonly PlayerManager remotePlayerManager = new PlayerManager();
        private static readonly PlayerVitalsManager remotePlayerVitalsManager = new PlayerVitalsManager();
        private static readonly PlayerChatManager remotePlayerChatManager = new PlayerChatManager();

        public static Dictionary<Type, PacketProcessor> PacketProcessorsByType;

        // List of arguments that can be used in a processor:
        private static readonly Dictionary<Type, object> processorArguments = new Dictionary<Type, object>
        {
            { typeof(PlayerManager), remotePlayerManager },
            { typeof(PlayerVitalsManager), remotePlayerVitalsManager },
            { typeof(PlayerChatManager), remotePlayerChatManager },
            { typeof(IPacketSender), clientBridge },
            { typeof(ClientBridge), clientBridge }
        };

        public string IpAddress { get; private set; }

        public string PlayerName { get; private set; }

        public static event Action OnBeforeMultiplayerStart;

        public void Awake()
        {
            Log.Info("Initializing Multiplayer Client...");

            PacketProcessorsByType = PacketProcessor.GetProcessors(processorArguments,
                p => p.BaseType.IsGenericType &&
                     p.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>));

            NitroxConsole.Main.AddCommand(ConsoleCommandWarpTo);
            NitroxConsole.Main.AddCommand(ConsoleCommandDisconnect);

            synchronizers = new GameObject("Synchronizers");
            synchronizers.SetActive(false);
            synchronizers.AddComponent<Chat>();
            synchronizers.AddComponent<PlayerMovement>();
            synchronizers.AddComponent<PlayerStatsBroadcaster>();
            synchronizers.AddComponent<AnimationSender>();
            synchronizers.AddComponent<EntityPositionBroadcaster>();
            synchronizers.transform.SetParent(transform);

            Main = this;
            Log.Info("Multiplayer Client Initialized...");
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

        [NitroxCommand("disconnect")]
        private void ConsoleCommandDisconnect(CommandEventArgs e)
        {
            enabled = false;
        }

        [NitroxCommand("warpto")]
        [NitroxCommandArg("other", CommandArgInput.Type.STRING, true, "o")]
        private void ConsoleCommandWarpTo(CommandEventArgs e)
        {
            Optional<string> otherPlyId = e.Get<string>("other");

            if (!otherPlyId.Then(plyId => remotePlayerManager.Find(plyId).Then(opPlayer =>
            {
                Player.main.SetPosition(opPlayer.Body.transform.position);
                Player.main.OnPlayerPositionCheat();
            })))
            {
                e.Error($"Cannot locate player by id: {otherPlyId}");
                return;
            }

            e.HandlerMessage = $"Teleported to player with id: {otherPlyId}";
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(PlayerName) || string.IsNullOrEmpty(IpAddress))
            {
                enabled = false;
                throw new NotSupportedException($"Multiplayer tried to start without a valid playername and/or ipadress set. Player: '{PlayerName}', IP: '{IpAddress}'");
            }

            StartCoroutine(EnableMultiplayerServices());
        }

        private IEnumerator EnableMultiplayerServices()
        {
            IsLoading = true;

            if (SceneManager.GetActiveScene().name == "StartScreen")
            {
                Log.InGame("Launching game...");
#pragma warning disable CS0618 // Type or member is obsolete
                IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
#pragma warning restore CS0618 // Type or member is obsolete
                StartCoroutine(startNewGame);

                Log.InGame("Waiting for game to load...");
                yield return new WaitUntil(() => LargeWorldStreamer.main != null);
                yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
                yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);
                yield return new WaitUntil(() => Player.main != null);
            }

            try
            {
                Log.InGame("Connecting to server...");
                // TODO: This is relatively slow and should be done over multiple frames (so that game doesn't stutter).
                clientBridge.Connect(IpAddress, PlayerName);
            }
            catch (ClientConnectionFailedException)
            {
                Log.InGame($"Unable to connect to server {IpAddress}.");
                IsLoading = false;
                enabled = false;
                throw;
            }

            Log.InGame("Waiting for reservation...");
            yield return new WaitUntil(() => clientBridge.CurrentState != ClientBridgeState.WaitingForRerservation);
            
            OnBeforeMultiplayerStart?.Invoke();
            clientBridge.ClaimReservation();

            switch (clientBridge.CurrentState)
            {
                case ClientBridgeState.Reserved:
                    Log.InGame("Reservation accepted.");                    
                    synchronizers.SetActive(true);
                    break;
                case ClientBridgeState.ReservationRejected:
                    Log.InGame("Reservation rejected...");
                    break;
            }

            IsLoading = false;
        }

        private void OnDisable()
        {
            if (IsLoading)
            {
                throw new NotSupportedException("Cannot disable Multiplayer when it is still loading.");
            }

            synchronizers.SetActive(false);
            remotePlayerManager.RemoveAllPlayers();
            clientBridge.Disconnect();
        }

        public void SetUserData(string ip, string username)
        {
            if (enabled)
            {
                throw new NotSupportedException("Cannot change user data while multiplayer is enabled.");
            }
            if (IsLoading)
            {
                throw new NotSupportedException("Cannot change user data while multiplayer is loading.");
            }
            Validate.NotNull(ip);
            Validate.NotNull(username);

            IpAddress = ip;
            PlayerName = username;
        }
    }
}
