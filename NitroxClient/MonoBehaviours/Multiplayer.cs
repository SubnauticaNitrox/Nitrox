using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxReloader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
{
    public class Multiplayer : MonoBehaviour
    {
        public static Multiplayer Main;

        private IMultiplayerSession multiplayerSession;
        private DeferringPacketReceiver packetReceiver;
        public static event Action OnBeforeMultiplayerStart;
        public static event Action OnAfterMultiplayerEnd;

        public bool InitialSyncCompleted;

        public void Awake()
        {
            Log.InGame("Multiplayer Client Loaded...");
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            packetReceiver = NitroxServiceLocator.LocateService<DeferringPacketReceiver>();
            Main = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            Reloader.ReloadAssemblies();
            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.Disconnected)
            {
                ProcessPackets();
            }
        }

        public bool IsMultiplayer()
        {
            return multiplayerSession.Client.IsConnected;
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

        public void StartSession()
        {
            DevConsole.RegisterConsoleCommand(this, "execute", false, false);
            OnBeforeMultiplayerStart?.Invoke();
            InitializeLocalPlayerState();
            multiplayerSession.JoinSession();
            InitMonoBehaviours();
            Utils.SetContinueMode(true);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnConsoleCommand_execute(NotificationCenter.Notification n)
        {
            string[] args = new string[n.data.Values.Count];
            n.data.Values.CopyTo(args, 0);

            NitroxServiceLocator.LocateService<IPacketSender>().Send(new ServerCommand(args));
        }

        private void InitializeLocalPlayerState()
        {
            ILocalNitroxPlayer localPlayer = NitroxServiceLocator.LocateService<ILocalNitroxPlayer>();
            PlayerModelDirector playerModelDirector = new PlayerModelDirector(localPlayer);
            playerModelDirector
                .AddDiveSuit();

            playerModelDirector.Construct();
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
        }

        public void StopCurrentSession()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
			
            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.Disconnected)
            {
                multiplayerSession.Disconnect();
            }
			
            OnAfterMultiplayerEnd?.Invoke();

            //Always do this last.
            NitroxServiceLocator.EndCurrentLifetimeScope();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                // If we just disconnected from a multiplayer session, then we need to kill the connection here.
                // Maybe a better place for this, but here works in a pinch.
                StopCurrentSession();
            }
        }

        public static void SubnauticaLoadingCompleted()
        {
            if (Main != null && Main.IsMultiplayer())
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
            WaitScreen.Item item = WaitScreen.Add("Loading Multiplayer", null);
            WaitScreen.ShowImmediately();
            Main.StartSession();
            yield return new WaitUntil(() => Main.InitialSyncCompleted);
            WaitScreen.Remove(item);
            SetLoadingComplete();
        }

        private static void SetLoadingComplete()
        {
            PropertyInfo property = PAXTerrainController.main.GetType().GetProperty("isWorking");
            property.SetValue(PAXTerrainController.main, false, null);

            WaitScreen waitScreen = (WaitScreen)ReflectionHelper.ReflectionGet<WaitScreen>(null, "main", false, true);
            waitScreen.ReflectionCall("Hide");

            HashSet<WaitScreen.Item> items = (HashSet<WaitScreen.Item>)waitScreen.ReflectionGet("items");
            items.Clear();
        }
    }
}
