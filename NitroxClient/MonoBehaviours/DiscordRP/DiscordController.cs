using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DiscordJoinEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordSpectateEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordJoinRequestEvent : UnityEngine.Events.UnityEvent<DiscordRpc.JoinRequest> { }

namespace NitroxClient.MonoBehaviours.DiscordRP
{
    public class DiscordController : MonoBehaviour
    {
        public DiscordRpc.RichPresence Presence;
        public string ApplicationId = "405122994348752896";
        public string OptionalSteamId = "264710";
        private int callbackCalls;
        public int ClickCounter;
        public UnityEngine.Events.UnityEvent OnConnect;
        public UnityEngine.Events.UnityEvent OnDisconnect;
        public DiscordJoinEvent OnJoin;
        public DiscordJoinEvent OnSpectate;
        public DiscordJoinRequestEvent OnJoinRequest;
        public bool ShowingWindow;

        DiscordRpc.JoinRequest lastJoinRequest;
        DiscordRpc.EventHandlers handlers;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }  

        public int CallbackCalls
        {
            get
            {
                return callbackCalls;
            }

            set
            {
                callbackCalls = value;
            }
        }

        public void ReadyCallback()
        {
            ++CallbackCalls;
            Log.Info("Discord: ready");
            if (OnConnect != null)
            {
                OnConnect.Invoke();
            }
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            ++CallbackCalls;
            Log.Info(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
            OnDisconnect.Invoke();
        }

        public void ErrorCallback(int errorCode, string message)
        {
            ++CallbackCalls;
            Log.Error(string.Format("Discord: error {0}: {1}", errorCode, message));
        }

        public void JoinCallback(string secret)
        {
            ++CallbackCalls;
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
            ++CallbackCalls;
            Log.Info(string.Format("Discord: spectate ({0})", secret));
            OnSpectate.Invoke(secret);
        }

        public void RequestCallback(ref DiscordRpc.JoinRequest request)
        {
            ++CallbackCalls;
            if (!ShowingWindow)
            {
                Log.Info(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
                AcceptRequest acceptRequest = gameObject.AddComponent<AcceptRequest>();
                acceptRequest.Request = request;
                lastJoinRequest = request;
                ShowingWindow = true;
            } else
            {
                Log.Info("Discord: Request window is allready active.");
            }
            OnJoinRequest.Invoke(request);
        }

        void Update()
        {
            DiscordRpc.RunCallbacks();
        }

        void OnEnable()
        {
            Log.Info("Discord: init");
            CallbackCalls = 0;
            ShowingWindow = false;

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.spectateCallback += SpectateCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(ApplicationId, ref handlers, true, OptionalSteamId);
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
            Presence.joinSecret = ipAddressPort;
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

        private void SendDRP() {
            Presence.largeImageKey = "diving";
            Presence.instance = false;
            DiscordRpc.UpdatePresence(ref Presence);
        }

        public void RespondLastJoinRequest(int accept)
        {
            DiscordRpc.Respond(lastJoinRequest.userId, (DiscordRpc.Reply)accept);
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
    }
}
