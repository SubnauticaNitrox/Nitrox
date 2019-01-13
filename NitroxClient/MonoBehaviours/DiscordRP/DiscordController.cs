using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.DiscordRP
{
    public class DiscordController : MonoBehaviour
    {
        public DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
        public bool ShowingWindow;

        private string applicationId = "405122994348752896";
        private string optionalSteamId = "264710";
        private string lastJoinRequestUserID;
        private DiscordRpc.EventHandlers handlers;
        private static DiscordController main;

        public static DiscordController Main
        {
            get
            {
                if (main == null)
                {
                    main = new GameObject("DiscordController").AddComponent<DiscordController>();
                }
                return main;
            }
            private set
            {
                main = value;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
        {
            Log.Info("Discord: ready");
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Log.Info(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Log.Error(string.Format("Discord: error {0}: {1}", errorCode, message));
        }

        public void JoinCallback(string secret)
        {
            Log.Info(string.Format("Discord: join ({0})", secret));
            Log.Debug("Discord" + SceneManager.GetActiveScene().name);
            if (SceneManager.GetActiveScene().name == "StartScreen")
            {
                NitroxServiceLocator.BeginNewLifetimeScope();
                JoinServer.SaveGameMenuPrototype = FindObject(MainMenuRightSide.main.gameObject, "SavedGames");
                JoinServer joinServer = gameObject.AddComponent<JoinServer>();
                string[] serverIpPort = secret.Split(':');
                joinServer.ServerIp = serverIpPort[0];
                joinServer.serverPort = int.Parse(serverIpPort[1]);
            }
            else
            {
                Log.InGame("Please be in the mainmenu if you want to join a session.");
                Log.Warn("Discord: Can't join a server outside of the mainmenu.");
            }
        }

        public void SpectateCallback(string secret)
        {
            Log.Info(string.Format("Discord: spectate ({0})", secret));
        }

        public void RequestCallback(ref DiscordRpc.DiscordUser request)
        {
            if (!ShowingWindow)
            {
                Log.Info(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
                DiscordJoinRequestGui acceptRequest = gameObject.AddComponent<DiscordJoinRequestGui>();
                acceptRequest.Request = request;
                lastJoinRequestUserID = request.userId;
                ShowingWindow = true;
            }
            else
            {
                Log.Info("Discord: Request window is allready active.");
            }
        }

        void Update()
        {
            DiscordRpc.RunCallbacks();
        }

        void OnEnable()
        {
            Log.Info("Discord: init");
            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.spectateCallback += SpectateCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);

            ShowingWindow = false;
        }

        void OnDisable()
        {
            Log.Info("Discord: shutdown");
            DiscordRpc.Shutdown();
        }

        public void InitDRPDiving(string username, int playercount, string ipAddressPort)
        {
            Presence.state = "Diving in game";
            Presence.details = "Playing as " + username;
            Presence.startTimestamp = 0;
            Presence.partyId = username;
            Presence.partySize = playercount;
            Presence.partyMax = 99;
            Presence.joinSecret = CheckIP(ipAddressPort);
            SendDRP();
        }

        public void InitDRPMenu()
        {
            Presence.state = "In menu";
            SendDRP();
        }

        public void UpdateDRPDiving(int playerCount)
        {
            Presence.partySize = playerCount;
            SendDRP();
        }

        private void SendDRP()
        {
            Presence.largeImageKey = "diving";
            Presence.instance = false;
            DiscordRpc.UpdatePresence(Presence);
        }

        public void RespondLastJoinRequest(int accept)
        {
            Log.Info("Discord: responding to Join request => " + (accept.Equals("1") ? "true" : "false"));
            DiscordRpc.Respond(lastJoinRequestUserID, (DiscordRpc.Reply)accept);
        }

        private GameObject FindObject(GameObject parent, string name)
        {
            Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
            foreach (Component t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        private string CheckIP(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];

            if (ip == "127.0.0.1")
            {
                return IPHelper.GetPublicIP() + ":" + port;
            }
            else
            {
                return ipPort;
            }
        }
    }
}
