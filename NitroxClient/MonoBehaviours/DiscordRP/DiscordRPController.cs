using System;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.DiscordRP
{
    public class DiscordRPController : MonoBehaviour
    {
        private const string APPLICATION_ID = "405122994348752896";
        private static DiscordRPController main;
        private bool showingWindow;
        private DiscordRpc.RichPresence presence;

        public static DiscordRPController Main
        {
            get
            {
                if (main == null)
                {
                    main = new GameObject("DiscordController").AddComponent<DiscordRPController>();
                    main.presence = new DiscordRpc.RichPresence();
                }
                return main;
            }
        }

        public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser) => Log.Info("[Discord] Ready");

        public void DisconnectedCallback(int errorCode, string message) => Log.Info($"[Discord] Disconnected: {errorCode} <=> {message}");

        public void ErrorCallback(int errorCode, string message) => Log.Error($"[Discord] Error: {errorCode} <=> {message}");

        public void JoinCallback(string secret)
        {
            Log.Info("[Discord] Joining Server");
            if (SceneManager.GetActiveScene().name == "StartScreen")
            {
                MainMenuMultiplayerPanel.OpenJoinServerMenu(secret);
            }
            else
            {
                Log.InGame("Please enter the multiplayer-main-menu if you want to join a session.");
                Log.Warn("[Discord] Warn: Can't join a server outside of the main-menu.");
            }
        }

        public void RequestCallback(ref DiscordRpc.DiscordUser request)
        {
            if (!showingWindow)
            {
                Log.Info($"[Discord] JoinRequest: Name:{request.username}#{request.discriminator} UserID:{request.userId}");
                DiscordJoinRequestGui acceptRequest = gameObject.AddComponent<DiscordJoinRequestGui>();
                acceptRequest.Request = request;
                showingWindow = true;
            }
            else
            {
                Log.Debug("[Discord] Request window is already active.");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (DiscordRpc.IsInitialized)
            {
                try
                {
                    DiscordRpc.RunCallbacks();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Discord RPC Controller threw an exception");
                }
            }
        }

        private void OnEnable()
        {
            Log.Info("[Discord] Init");
            DiscordRpc.EventHandlers handlers = new DiscordRpc.EventHandlers
            {
                readyCallback = ReadyCallback
            };
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(APPLICATION_ID, ref handlers, true, "");
        }

        private void OnDisable()
        {
            Log.Info("[Discord] Shutdown");
            DiscordRpc.Shutdown();
        }

        public void InitializeInGame(string username, int playerCount, string ipAddressPort)
        {
            presence.state = "In game";
            presence.details = "Playing as " + username;
            presence.startTimestamp = 0;
            presence.partyId = "PartyID:" + CheckIP(ipAddressPort);
            presence.partySize = playerCount;
            presence.partyMax = 100;
            presence.joinSecret = CheckIP(ipAddressPort);
            SendRP();
        }

        public void InitializeMenu()
        {
            presence.state = "In menu";
            SendRP();
        }

        public void UpdatePlayerCount(int playerCount)
        {
            presence.partySize = playerCount;
            SendRP();
        }

        private void SendRP()
        {
            presence.largeImageKey = "icon";
            presence.instance = false;
            DiscordRpc.UpdatePresence(presence);
        }

        public void RespondJoinRequest(string userID, DiscordRpc.Reply reply)
        {
            Log.Info($"[Discord] Respond JoinRequest: {userID} responded with {reply:g}");
            showingWindow = false;
            DiscordRpc.Respond(userID, reply);
        }

        private static string CheckIP(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];

            if (ip == "127.0.0.1")
            {
                return WebHelper.GetPublicIP() + ":" + port;
            }
            else
            {
                return ipPort;
            }
        }
    }
}
