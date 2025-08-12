using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main;
        private readonly Dictionary<Type, PacketProcessor> packetProcessorCache = new();
        private IClient client;
        private IMultiplayerSession multiplayerSession;
        private PacketReceiver packetReceiver;
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
            NitroxServiceLocator.LifetimeScopeEnded += (_, _) => packetProcessorCache.Clear();
            client = NitroxServiceLocator.LocateService<IClient>();
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();
            throttledPacketSender = NitroxServiceLocator.LocateService<ThrottledPacketSender>();
            terrain = NitroxServiceLocator.LocateService<GameLogic.Terrain>();

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

            WaitScreen.ManualWaitItem item = WaitScreen.Add(Language.main.Get("Nitrox_JoiningSession"));
            yield return Main.StartCoroutine(Main.StartSession());
            WaitScreen.Remove(item);

            yield return new WaitUntil(() => Main.InitialSyncCompleted);

            SetLoadingComplete();
            OnLoadingComplete?.Invoke();
        }

        public void ProcessPackets()
        {
            static PacketProcessor ResolveProcessor(Packet packet, Dictionary<Type, PacketProcessor> processorCache)
            {
                Type packetType = packet.GetType();
                if (processorCache.TryGetValue(packetType, out PacketProcessor processor))
                {
                    return processor;
                }

                try
                {
                    Type packetProcessorType = typeof(ClientPacketProcessor<>).MakeGenericType(packetType);
                    return processorCache[packetType] = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to find packet processor for packet {packet}");
                }

                return null;
            }

            packetReceiver.ConsumePackets(static (packet, processorCache) =>
            {
                try
                {
                    ResolveProcessor(packet, processorCache)?.ProcessPacket(packet, null);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error while processing packet {packet}");
                }
            }, packetProcessorCache);
        }

        public IEnumerator StartSession()
        {
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
            OnAfterMultiplayerEnd?.Invoke();
        }

        private static void SetLoadingComplete()
        {
#if SUBNAUTICA
            WaitScreen.main.isWaiting = false;
            WaitScreen.main.stageProgress.Clear();
            FreezeTime.End(FreezeTime.Id.WaitScreen);
#elif BELOWZERO
            WaitScreen.main.Hide();
#endif
            WaitScreen.main.items.Clear();

            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();

            LoadingScreenVersionText.DisableWarningText();
            DiscordClient.InitializeRPInGame(Main.multiplayerSession.AuthenticationContext.Username, remotePlayerManager.GetTotalPlayerCount(), Main.multiplayerSession.SessionPolicy.MaxConnections);
            CoroutineHost.StartCoroutine(NitroxServiceLocator.LocateService<PlayerChatManager>().LoadChatKeyHint());
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
                SceneCleaner.Open();
            }
        }
    }
}
