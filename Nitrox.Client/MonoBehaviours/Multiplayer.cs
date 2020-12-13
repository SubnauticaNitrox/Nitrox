using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Nitrox.Client.Communication;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.MultiplayerSession;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.ChatUI;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.GameLogic.PlayerModel.ColorSwap;
using Nitrox.Client.MonoBehaviours.DiscordRP;
using Nitrox.Client.MonoBehaviours.Gui.InGame;
using Nitrox.Client.MonoBehaviours.Gui.MainMenu;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Model.Subnautica.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nitrox.Client.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main;

        private IMultiplayerSession multiplayerSession;
        private PacketReceiver packetReceiver;
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
            WaitScreen.ManualWaitItem worldSettleItem = WaitScreen.Add("Awaiting World Settling");
            WaitScreen.ShowImmediately();

            yield return new WaitUntil(() => LargeWorldStreamer.main != null &&
                                             LargeWorldStreamer.main.land != null &&
                                             LargeWorldStreamer.main.IsReady() &&
                                             LargeWorldStreamer.main.IsWorldSettled());

            WaitScreen.Remove(worldSettleItem);

            WaitScreen.ManualWaitItem item = WaitScreen.Add("Joining Multiplayer Session");
            yield return Main.StartCoroutine(Main.StartSession());
            WaitScreen.Remove(item);

            yield return new WaitUntil(() => Main.InitialSyncCompleted);

            SetLoadingComplete();
        }

        public void Awake()
        {
            Log.InGame("Multiplayer Client Loaded...");

            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();
            Model.Helper.Map.Main = new SubnauticaMap();
            Main = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
            {
                ProcessPackets();
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
                    processor.ProcessPacket(packet, null);
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
            gameObject.AddComponent<PlayerMovement>();
            gameObject.AddComponent<PlayerDeathBroadcaster>();
            gameObject.AddComponent<PlayerStatsBroadcaster>();
            gameObject.AddComponent<AnimationSender>();
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
            PropertyInfo property = PAXTerrainController.main.GetType().GetProperty("isWorking");
            property.SetValue(PAXTerrainController.main, false, null);

            WaitScreen waitScreen = (WaitScreen)ReflectionHelper.ReflectionGet<WaitScreen>(null, "main", false, true);
            waitScreen.ReflectionCall("Hide");

            List<WaitScreen.IWaitItem> items = (List<WaitScreen.IWaitItem>)waitScreen.ReflectionGet("items");
            items.Clear();

            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();

            LoadingScreenVersionText.DisableWarningText();
            DiscordRPController.Main.InitializeInGame(Main.multiplayerSession.AuthenticationContext.Username, remotePlayerManager.GetTotalPlayerCount(), Main.multiplayerSession.SessionPolicy.MaxConnections, $"{Main.multiplayerSession.IpAddress}:{Main.multiplayerSession.ServerPort}");
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
            HashSet<string> whiteListedSerializableComponents = (HashSet<string>)ReflectionHelper.ReflectionGet<ProtobufSerializer>(null, "componentWhitelist", false, true);
            whiteListedSerializableComponents.Add("NitroxEntity");
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
