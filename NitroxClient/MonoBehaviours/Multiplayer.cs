using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main;
        private ClientProcessorContext packetProcessorContext;
        private PacketProcessorsInvoker processorInvoker = null!;
        private IClient client;
        private IMultiplayerSession multiplayerSession;
        private PacketReceiver packetReceiver;
        private IPacketSender packetSender;
        private ThrottledPacketSender throttledPacketSender;
        private GameLogic.Terrain terrain;

        public bool InitialSyncCompleted { get; set; }

        /// <summary>
        ///     True if multiplayer is loaded and client is connected to a server.
        /// </summary>
        public static bool Active => Main && Main.multiplayerSession.Client.IsConnected;

        /// <summary>
        ///     True if multiplayer is loaded and player has successfully joined a server.
        /// </summary>
        public static bool Joined => Main && Main.multiplayerSession.CurrentState.CurrentStage == MultiplayerSessionConnectionStage.SESSION_JOINED;

        public void Awake()
        {
            client = NitroxServiceLocator.LocateService<IClient>();
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            throttledPacketSender = NitroxServiceLocator.LocateService<ThrottledPacketSender>();
            terrain = NitroxServiceLocator.LocateService<GameLogic.Terrain>();
            packetProcessorContext = new ClientProcessorContext(packetSender);
            processorInvoker = NitroxServiceLocator.LocateService<PacketProcessorsInvoker>();

            Main = this;
            DontDestroyOnLoad(gameObject);

            Log.Info("Multiplayer client loaded…");
            Log.InGame(Language.main.Get("Nitrox_MultiplayerLoaded"));
        }

        public void Update()
        {
            client.PollEvents();

            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
            {
                ProcessPackets();
                throttledPacketSender.Update();

                // Loading up shouldn't be bothered by entities spawning in the surroundings
                if (multiplayerSession.CurrentState.CurrentStage == MultiplayerSessionConnectionStage.SESSION_JOINED &&
                    InitialSyncCompleted)
                {
                    terrain.UpdateVisibility();
                }
            }
        }

        public static event Action OnLoadingComplete;
        public static event Action OnBeforeMultiplayerStart;
        public static event Action OnAfterMultiplayerEnd;

        public static void SubnauticaLoadingStarted()
        {
            OnBeforeMultiplayerStart?.Invoke();
        }

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
                OnLoadingComplete?.Invoke();
            }
        }

        public static IEnumerator LoadAsync()
        {
            WaitScreen.ManualWaitItem worldSettleItem = WaitScreen.Add(Language.main.Get("Nitrox_WorldSettling"));

            yield return new WaitUntil(() => LargeWorldStreamer.main != null &&
                                             LargeWorldStreamer.main.land != null &&
                                             LargeWorldStreamer.main.IsReady() &&
                                             LargeWorldStreamer.main.IsWorldSettled());

            WaitScreen.Remove(worldSettleItem);

            WaitScreen.ManualWaitItem joiningItem = WaitScreen.Add(Language.main.Get("Nitrox_JoiningSession"));
            yield return Main.StartCoroutine(Main.StartSession());
            WaitScreen.Remove(joiningItem);

            WaitScreen.ManualWaitItem waitingItem = WaitScreen.Add(Language.main.Get("Nitrox_Waiting"));
            Log.InGame(Language.main.Get("Nitrox_Waiting"));
            yield return new WaitUntil(() => Main.InitialSyncCompleted);
            WaitScreen.Remove(waitingItem);

            SetLoadingComplete();
            OnLoadingComplete?.Invoke();
        }

        public void ProcessPackets()
        {
            packetReceiver.ConsumePackets(static (packet, context) =>
            {
                try
                {
                    PacketProcessorsInvoker.Entry processor = context.processorInvoker.GetProcessor(packet.GetType());
                    if (processor == null)
                    {
                        throw new Exception($"Failed to find packet processor for packet {packet.GetType()}");
                    }
                    processor.Execute(context.packetProcessorContext, packet).ContinueWithHandleError();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error trying to process packet {packet}");
                }
            }, (processorInvoker, packetSender, packetProcessorContext));
        }

        public IEnumerator StartSession()
        {
            yield return StartCoroutine(InitializeLocalPlayerState());
            multiplayerSession.JoinSession();
            InitMonoBehaviours();
            Utils.SetContinueMode(true);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            RegisterConnectedDelegates();
        }

        public void InitMonoBehaviours()
        {
            // Gameplay.
            gameObject.AddComponent<UnderwaterStateTracker>();
            gameObject.AddComponent<PrecursorTracker>();
            gameObject.AddComponent<PlayerMovementBroadcaster>();
            gameObject.AddComponent<PlayerDeathBroadcaster>();
            gameObject.AddComponent<PlayerStatsBroadcaster>();
            gameObject.AddComponent<EntityPositionBroadcaster>();
            gameObject.AddComponent<BuildingHandler>();
            gameObject.AddComponent<MovementBroadcaster>();
            VirtualCyclops.Initialize();
        }

        public void StopCurrentSession()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            // Destroy session-scoped Mono's before cleanup events
            DestroySessionMonoBehaviours();

            // Clear entity registry before invoking end event
            NitroxEntity.ClearAll();

            // clear remote players
            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            remotePlayerManager.RemoveAllPlayers();

            OnAfterMultiplayerEnd?.Invoke();

            UnregisterConnectedDelegates();

            // Reset state
            InitialSyncCompleted = false;
        }

        /// <summary>
        /// Destroys session-scoped MonoBehaviours to clean up resources before multiplayer end.
        /// todo: consider replacing manual Destroy calls by having components inherit
        /// <see cref="NitroxSessionBehaviour"/> so cleanup happens automatically on session end.
        /// </summary>
        private void DestroySessionMonoBehaviours()
        {
            Destroy(GetComponent<UnderwaterStateTracker>());
            Destroy(GetComponent<PrecursorTracker>());
            Destroy(GetComponent<PlayerMovementBroadcaster>());
            Destroy(GetComponent<PlayerDeathBroadcaster>());
            Destroy(GetComponent<PlayerStatsBroadcaster>());
            Destroy(GetComponent<EntityPositionBroadcaster>());
            Destroy(GetComponent<BuildingHandler>());
            Destroy(GetComponent<MovementBroadcaster>());
        }

        private static void SetLoadingComplete()
        {
            WaitScreen.main.isWaiting = false;
            WaitScreen.main.stageProgress.Clear();
            FreezeTime.End(FreezeTime.Id.WaitScreen);
            WaitScreen.main.items.Clear();

            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();

            TopRightWatermarkText.ApplyChangesForInGame();
            DiscordClient.InitializeRPInGame(Main.multiplayerSession.AuthenticationContext.Username, remotePlayerManager.GetTotalPlayerCount(), Main.multiplayerSession.SessionPolicy.MaxConnections);
            CoroutineHost.StartCoroutine(PlayerChatManager.Instance.LoadChatKeyHint());
        }

        private IEnumerator InitializeLocalPlayerState()
        {
            ILocalNitroxPlayer localPlayer = NitroxServiceLocator.LocateService<ILocalNitroxPlayer>();
            IEnumerable<IColorSwapManager> colorSwapManagers = NitroxServiceLocator.LocateService<IEnumerable<IColorSwapManager>>();

            // This is used to init the lazy GameObject in order to create a real default Body Prototype for other players
            GameObject body = localPlayer.BodyPrototype;
            Log.Info($"Init body prototype {body.name}");

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
                JoinServerBackend.StopMultiplayerClient();
            }
        }

        private void OnPlayerChat(string message)
        {
            multiplayerSession.Send(new ChatMessage(multiplayerSession.Reservation.SessionId, message));
        }

        private void OnPlayerCommand(string command)
        {
            multiplayerSession.Send(new ServerCommand(command));
        }

        public void RegisterConnectedDelegates()
        {
            PlayerChatManager.Instance.OnPlayerChat += OnPlayerChat;
            PlayerChatManager.Instance.OnPlayerCommand += OnPlayerCommand;
        }

        public void UnregisterConnectedDelegates()
        {
            PlayerChatManager.Instance.OnPlayerChat -= OnPlayerChat;
            PlayerChatManager.Instance.OnPlayerCommand -= OnPlayerCommand;
        }
    }
}
