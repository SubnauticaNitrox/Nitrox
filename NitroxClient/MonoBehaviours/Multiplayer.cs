using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main;

        private IMultiplayerSession multiplayerSession;
        private IPacketSender packetSender;
        private PacketReceiver packetReceiver;
        private ThrottledPacketSender throttledPacketSender;
        public bool InitialSyncCompleted { get; set; }

        /// <summary>
        ///     True if multiplayer is loaded and client is connected to a server.
        /// </summary>
        public static bool Active => Main != null && Main.multiplayerSession.Client.IsConnected;

        public static event Action OnBeforeMultiplayerStart;
        public static event Action OnAfterMultiplayerEnd;

        public static void SubnauticaLoadingCompleted()
        {
            if (Active)
            {
                Main.InitialSyncCompleted = false;
                Main.StartCoroutine(LoadAsync());
            }
            else
            {
                SetLoadingComplete();
            }
        }

        public static IEnumerator LoadAsync()
        {
            WaitScreen.ManualWaitItem worldSettleItem = WaitScreen.Add(Language.main.Get("Nitrox_WorldSettling"));
            WaitScreen.ShowImmediately();

            yield return new WaitUntil(() => LargeWorldStreamer.main != null &&
                                             LargeWorldStreamer.main.land != null &&
                                             LargeWorldStreamer.main.IsReady() &&
                                             LargeWorldStreamer.main.IsWorldSettled());

            WaitScreen.Remove(worldSettleItem);

            WaitScreen.ManualWaitItem item = WaitScreen.Add(Language.main.Get("Nitrox_JoiningSession"));
            yield return Main.StartCoroutine(Main.StartSession());
            WaitScreen.Remove(item);

            yield return new WaitUntil(() => Main.InitialSyncCompleted);

            SetLoadingComplete();
        }

        public void Awake()
        {
            Log.InGame(Language.main.Get("Nitrox_MultiplayerLoaded"));

            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();
            throttledPacketSender = NitroxServiceLocator.LocateService<ThrottledPacketSender>();

            Main = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
            {
                ProcessPackets();
                throttledPacketSender.Update();
            }
        }

        public void ProcessPackets()
        {
            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            foreach (Packet packet in packets)
            {
                try
                {
                    Type clientPacketProcessorType = typeof(ClientPacketProcessor<>);
                    Type packetType = packet.GetType();
                    Type packetProcessorType = clientPacketProcessorType.MakeGenericType(packetType);

                    PacketProcessor processor = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);

                    // Incoming packets may trigger new sounds that we don't want to spread over the network
                    using (packetSender.SuppressSounds())
                    {
                        processor.ProcessPacket(packet, null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error processing packet {packet}");
                }
            }
        }

        public IEnumerator StartSession()
        {
            DevConsole.RegisterConsoleCommand(this, "execute");
            OnBeforeMultiplayerStart?.Invoke();
            yield return StartCoroutine(InitializeLocalPlayerState());
            multiplayerSession.JoinSession();
            InitMonoBehaviours();
            Utils.SetContinueMode(true);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        public void InitMonoBehaviours()
        {
            // Gameplay.
            gameObject.AddComponent<AnimationSender>();
            gameObject.AddComponent<PlayerMovement>();
            gameObject.AddComponent<PlayerDeathBroadcaster>();
            gameObject.AddComponent<PlayerStatsBroadcaster>();
            gameObject.AddComponent<EntityPositionBroadcaster>();
            gameObject.AddComponent<ThrottledBuilder>();

            // UI.
            gameObject.AddComponent<LostConnectionModal>();
            gameObject.AddComponent<PlayerKickedModal>();
        }

        public void StopCurrentSession()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
            {
                multiplayerSession.Disconnect();
            }

            OnAfterMultiplayerEnd?.Invoke();

            //Always do this last.
            NitroxServiceLocator.EndCurrentLifetimeScope();
        }

        private static void SetLoadingComplete()
        {
            PAXTerrainController.main.isWorking = false;
            WaitScreen.main.Hide();
            WaitScreen.main.items.Clear();

            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();

            LoadingScreenVersionText.DisableWarningText();
            DiscordClient.InitializeRPInGame(Main.multiplayerSession.AuthenticationContext.Username, remotePlayerManager.GetTotalPlayerCount(), Main.multiplayerSession.SessionPolicy.MaxConnections);
            NitroxServiceLocator.LocateService<PlayerChatManager>().LoadChatKeyHint();
        }

        private void OnConsoleCommand_execute(NotificationCenter.Notification n)
        {
            string[] args = new string[n.data.Values.Count];
            n.data.Values.CopyTo(args, 0);

            NitroxServiceLocator.LocateService<IPacketSender>().Send(new ServerCommand(args));
        }

        private IEnumerator InitializeLocalPlayerState()
        {
            ILocalNitroxPlayer localPlayer = NitroxServiceLocator.LocateService<ILocalNitroxPlayer>();
            IEnumerable<IColorSwapManager> colorSwapManagers = NitroxServiceLocator.LocateService<IEnumerable<IColorSwapManager>>();

            ColorSwapAsyncOperation swapOperation = new ColorSwapAsyncOperation(localPlayer, colorSwapManagers).BeginColorSwap();
            yield return new WaitUntil(() => swapOperation.IsColorSwapComplete());

            swapOperation.ApplySwappedColors();

            // UWE developers added noisy logging for non-whitelisted components during serialization.
            // We add NitroxEntiy in here to avoid a large amount of log spam.
            ProtobufSerializer.componentWhitelist.Add(nameof(NitroxEntity));
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                // If we just disconnected from a multiplayer session, then we need to kill the connection here.
                // Maybe a better place for this, but here works in a pinch.
                StopCurrentSession();
                SceneCleaner.Open();
            }
        }
    }
}
