﻿using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class ConsoleJoinServer : MonoBehaviour
    {
        private IMultiplayerSession multiplayerSession;
        private GameObject multiplayerClient;
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";
        private string userName;

        public void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            DevConsole.RegisterConsoleCommand(this, "mplayer", false);
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
            DevConsole.RegisterConsoleCommand(this, "disconnect", false);
        }

        public void OnConsoleCommand_mplayer(NotificationCenter.Notification n)
        {
            //This could be cleaned up. Honestly, I see this as a hack to deal with other unimplemented features. I'd rather just patch this boat until we can let it sink...
            NitroxServiceLocator.BeginNewLifetimeScope();
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();

            if (multiplayerSession.CurrentState.CurrentStage == MultiplayerSessionConnectionStage.SessionJoined)
            {
                Log.InGame("Already connected to a server");
            }
            else if (n?.data?.Count > 0)
            {
                Log.InGame("Console Multiplayer Session Join Client Loaded...");
                StartMultiplayerClient();
                int serverPort = 11000;
                string ipAddress = n.data.Count >= 2 ? (string)n.data[1] : DEFAULT_IP_ADDRESS;
                userName = (string)n.data[0];
                string seperator = ":";
                if (ipAddress.Contains(seperator))
                {
                    char splitter = ':';
                    string[] splitIP = ipAddress.Split(splitter);
                    ipAddress = splitIP[0];
                    serverPort = int.Parse(splitIP[1]);
                }
                multiplayerSession.Connect(ipAddress,serverPort);
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
                Multiplayer.Main.StopCurrentSession();
                StopMultiplayerClient();
            }
        }

        public void OnConsoleCommand_warpto(NotificationCenter.Notification n)
        {
            if (n?.data?.Count > 0)
            {
                string otherPlayerName = (string)n.data[0];
                PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                Optional<RemotePlayer> opPlayer = remotePlayerManager.FindByName(otherPlayerName);
                if (opPlayer.IsPresent())
                {
                    Player.main.SetPosition(opPlayer.Get().Body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject();
                multiplayerClient.AddComponent<Multiplayer>();
                multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
            }
        }

        private void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.AwaitingReservationCredentials:
                    AuthenticationContext authenticationContext = new AuthenticationContext(userName);
                    multiplayerSession.RequestSessionReservation(new PlayerSettings(RandomColorGenerator.GenerateColor()), authenticationContext);
                    break;
                case MultiplayerSessionConnectionStage.SessionReserved:
                    multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                    Multiplayer.Main.StartSession();
                    break;
                case MultiplayerSessionConnectionStage.SessionReservationRejected:
                    Log.InGame($"Cannot join server: {multiplayerSession.Reservation.ReservationState.ToString()}");
                    multiplayerSession.Disconnect();
                    break;
                case MultiplayerSessionConnectionStage.Disconnected:
                    multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                    break;
                default:
                    Log.InGame($"Current Stage: {multiplayerSession.CurrentState.CurrentStage}");
                    break;
            }
        }

        private void StopMultiplayerClient()
        {
            if (multiplayerClient != null)
            {
                Multiplayer.Main.StopCurrentSession();
                Destroy(multiplayerClient);
                multiplayerClient = null;
            }
        }
    }
}
