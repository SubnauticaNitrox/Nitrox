using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Logger;
using UnityEngine;

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

        DiscordRpc.JoinRequest lastJoinRequest;
        bool showingWindow;
        DiscordRpc.EventHandlers handlers;

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
            JoinServer joinServer = gameObject.AddComponent<JoinServer>();
            if (LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled())
            {
                Multiplayer.Main.StartMultiplayer(secret, "Test Username");
            }
            else
            {
                StartCoroutine(joinServer.JoinServerWait(secret, "Test Username"));
            }
            OnJoin.Invoke(secret);
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
            if (!showingWindow)
            {
                Log.Info(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
                AcceptRequest acceptRequest = gameObject.AddComponent<AcceptRequest>();
                acceptRequest.Request = request;
                lastJoinRequest = request;
                showingWindow = true;
            }
            OnJoinRequest.Invoke(request);
        }

        public void RequestCallbackTest()
        {
            if (!showingWindow)
            {
                Log.Info(string.Format("Discord: join request {0}#{1}: {2}", "Thijmen", "Bal Bla", "#7494"));
                AcceptRequest acceptRequest = gameObject.AddComponent<AcceptRequest>();
                showingWindow = true;
            }
        }

        void Update()
        {
            DiscordRpc.RunCallbacks();
        }

        void OnEnable()
        {
            Log.Info("Discord: init");
            CallbackCalls = 0;
            showingWindow = false;

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


        public void UpdatePresence()
        {
            Presence.largeImageKey = "diving";
            Presence.instance = false;
            DiscordRpc.UpdatePresence(ref Presence);
        }

        public void RespondLastJoinRequest(int accept)
        {
            DiscordRpc.Respond(lastJoinRequest.userId, (DiscordRpc.Reply)accept);
        }
    }
}
