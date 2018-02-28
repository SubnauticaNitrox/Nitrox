﻿using System;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;
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
        public static Multiplayer Main;
        public static event Action OnBeforeMultiplayerStart;

        private IMultiplayerSession multiplayerSession;
        private DeferringPacketReceiver packetReceiver;

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
            InitMonoBehaviours();
            OnBeforeMultiplayerStart();
            multiplayerSession.JoinSession();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
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
            multiplayerSession.Disconnect();

            PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();
            remotePlayerManager.RemoveAllPlayers();

            packetReceiver.Flush();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                //If we just disconnected from a multiplayer session, then we need to kill the connection here.
                //Maybe a better place for this, but here works in a pinch.
                StopCurrentSession();
            }
        }
    }
}
