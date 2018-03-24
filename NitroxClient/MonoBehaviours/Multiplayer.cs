using System;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxModel.Core;
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

        public void StartSession()
        {
            OnBeforeMultiplayerStart?.Invoke();
            InitializeLocalPlayerState();
            multiplayerSession.JoinSession();
            InitMonoBehaviours();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
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
            gameObject.AddComponent<PlayerMovement>();
            gameObject.AddComponent<PlayerStatsBroadcaster>();
            gameObject.AddComponent<AnimationSender>();
            gameObject.AddComponent<EntityPositionBroadcaster>();
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

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                // If we just disconnected from a multiplayer session, then we need to kill the connection here.
                // Maybe a better place for this, but here works in a pinch.
                StopCurrentSession();
            }
        }
    }
}
