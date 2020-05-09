using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.DiscordRP
{
    public class DiscordRPController : MonoBehaviour
    {
        public DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
        public bool ShowingWindow;

        private const string APPLICATION_ID = "405122994348752896";
        private DiscordRpc.EventHandlers handlers;

        private static DiscordRPController main;
        public static DiscordRPController Main
        {
            get
            {
                if (main == null)
                {
                    main = new GameObject("DiscordController").AddComponent<DiscordRPController>();
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
            if (SceneManager.GetActiveScene().name == "StartScreen" && MainMenuMultiplayerPanel.Main != null)
            {
                MainMenuMultiplayerPanel.Main.OpenJoinServerMenu(secret);
            }
            else
            {
                Log.InGame("Please enter the multiplayer-main-menu if you want to join a session.");
                Log.Warn("[Discord] Warn: Can't join a server outside of the main-menu.");
            }
        }

        public void RequestCallback(ref DiscordRpc.DiscordUser request)
        {
            if (!ShowingWindow)
            {
                Log.Info($"[Discord] JoinRequest: Name:{request.username}#{request.discriminator} UserID:{request.userId}");
                DiscordJoinRequestGui acceptRequest = gameObject.AddComponent<DiscordJoinRequestGui>();
                acceptRequest.Request = request;
                ShowingWindow = true;
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
            DiscordRpc.RunCallbacks();
        }

        private void OnEnable()
        {
            Log.Info("[Discord] Init");
            handlers = new DiscordRpc.EventHandlers
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
            Presence.state = "In game";
            Presence.details = "Playing as " + username;
            Presence.startTimestamp = 0;
            Presence.partyId = "PartyID:" + CheckIP(ipAddressPort);
            Presence.partySize = playerCount;
            Presence.partyMax = 100;
            Presence.joinSecret = CheckIP(ipAddressPort);
            SendRP();
        }

        public void InitializeMenu()
        {
            Presence.state = "In menu";
            SendRP();
        }

        public void UpdatePlayerCount(int playerCount)
        {
            Presence.partySize = playerCount;
            SendRP();
        }

        private void SendRP()
        {
            Presence.largeImageKey = "icon";
            Presence.instance = false;
            DiscordRpc.UpdatePresence(Presence);
        }

        public void RespondJoinRequest(string userID, DiscordRpc.Reply reply)
        {
            Log.Info($"[Discord] Respond JoinRequest: {userID} responded with {reply:g}");
            ShowingWindow = false;
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
