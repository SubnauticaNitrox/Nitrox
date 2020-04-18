using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel_Subnautica.Helper;
using NitroxModel_Subnautica.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
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
            Log.InGameLogger = new SubnauticaInGameLogger();
            NitroxModel.Helper.Map.Main = new SubnauticaMap();
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
                    Log.Error("Error processing packet: " + packet, ex);
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
            DiscordRPController.Main.InitializeInGame(Main.multiplayerSession.AuthenticationContext.Username, remotePlayerManager.GetTotalPlayerCount(), Main.multiplayerSession.IpAddress + ":" + Main.multiplayerSession.ServerPort);
            PlayerChatManager.LoadChatKeyHint();
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
