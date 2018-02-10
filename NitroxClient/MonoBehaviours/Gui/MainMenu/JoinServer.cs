using System.Collections;
using NitroxClient.Unity.Helper;
using NitroxClient.Communication.MultiplayerSession;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        public string ServerIp = "";
        Rect joinServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        Rect unableToJoinWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        string username = "username";
        bool joiningServer;
        bool notifyingUnableToJoin;
        bool shouldFocus;

        private GameObject multiplayerClient;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            StartMultiplayerClient();
        }

        public void Start()
        {
            notifyingUnableToJoin = false;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (joiningServer)
            {
                joinServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, RenderJoinServerDialog, "Join server");
            }

            if (notifyingUnableToJoin)
            {
                unableToJoinWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), unableToJoinWindowRect, RenderUnableToJoinDialog, "Unable to Join Session");
            }
        }

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject();
                multiplayerClient.AddComponent<Multiplayer>();
            }

            Multiplayer.Logic.MultiplayerSessionManager.ConnectionStateChanged += state =>
            {
                switch (state.CurrentStage)
                {
                    case MultiplayerSessionConnectionStage.AwaitingReservationCredentials:
                        joiningServer = true;
                        break;
                    case MultiplayerSessionConnectionStage.AwaitingSessionReservation:
                        joiningServer = false;
                        break;
                    case MultiplayerSessionConnectionStage.SessionReserved:
                        Log.InGame("Launching game...");
                        StartCoroutine(LaunchSession());
                        break;
                    case MultiplayerSessionConnectionStage.SessionReservationRejected:
                        Log.InGame("Reservation rejected...");
                        notifyingUnableToJoin = true;
                        break;
                }
            };

            Multiplayer.Main.InitiateSessionConnection(ServerIp);
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

        private IEnumerator LaunchSession()
        {
            Log.InGame("Launching game...");

#pragma warning disable CS0618 // Type or member is obsolete
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
            StartCoroutine(startNewGame);

            Log.InGame("Waiting for game to load...");
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);

            Log.InGame("Joining Multiplayer Session...");
            Multiplayer.Main.JoinSession();

            Destroy(gameObject);
        }

        private void RenderJoinServerDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        Multiplayer.Main.RequestSessionReservation(new PlayerSettings(), new AuthenticationContext(username));
                        break;
                    case KeyCode.Escape:
                        StopMultiplayerClient();
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin("menus.server", 80), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Username:");
                        GUI.SetNextControlName("usernameField");
                        username = GUILayout.TextField(username);
                    }

                    if (GUILayout.Button("Join"))
                    {
                        Multiplayer.Main.RequestSessionReservation(new PlayerSettings(), new AuthenticationContext(username));
                    }

                    if (GUILayout.Button("Cancel"))
                    {
                        StopMultiplayerClient();
                    }
                }
            });

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }

        private void RenderUnableToJoinDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        joiningServer = true;
                        notifyingUnableToJoin = false;
                        break;
                    case KeyCode.Escape:
                        joiningServer = true;
                        notifyingUnableToJoin = false;
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin("dialogs.server.rejected", 490), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        MultiplayerSessionReservationState reservationState = Multiplayer.Logic.MultiplayerSessionManager.Reservation.ReservationState;
                        string reservationStateDescription = reservationState.Describe();

                        GUILayout.Label(reservationStateDescription);
                    }

                    if (GUILayout.Button("OK"))
                    {
                        notifyingUnableToJoin = false;
                        Multiplayer.Main.InitiateSessionConnection(ServerIp);
                    }
                }
            });
        }

        private GUISkin GetGUISkin(string skinName, int labelWidth)
        {
            return GUISkinUtils.RegisterDerivedOnce(skinName, s =>
            {
                s.textField.fontSize = 14;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleLeft;
                s.textField.wordWrap = true;
                s.textField.stretchHeight = true;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 14;
                s.label.alignment = TextAnchor.MiddleRight;
                s.label.stretchHeight = true;
                s.label.fixedWidth = labelWidth;

                s.button.fontSize = 14;
                s.button.stretchHeight = true;
            });
        }
    }
}
