using NitroxClient.Unity.Helper;
using System.Collections;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        public string ServerIp = "";
        Rect joinServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        Rect unableToJoinWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        string username = "username";
        bool joiningServer = false;
        bool notifyingUnableToJoin = false;
        bool shouldFocus;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            joiningServer = true;
            notifyingUnableToJoin = false;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (joiningServer)
            {
                joinServerWindowRect = GUILayout.Window(0, joinServerWindowRect, BuildJoinServerDialog, "Join server");
            }

            if (notifyingUnableToJoin)
            {
                unableToJoinWindowRect = GUILayout.Window(1, joinServerWindowRect, BuildUnableToJoinDialog, "Unable to Join Session");
            }
        }

        private IEnumerator NegotiateSession(string serverIp)
        {
            Multiplayer.Main.NegotiatePlayerSlotReservation(serverIp, username);
            
            yield return new WaitUntil(() => Multiplayer.Logic.ClientBridge.CurrentState != Communication.ClientBridgeState.WaitingForRerservation);

            switch (Multiplayer.Logic.ClientBridge.CurrentState)
            {
                case Communication.ClientBridgeState.Reserved:
                    StartCoroutine(LaunchSession());
                    Destroy(gameObject);
                    break;
                case Communication.ClientBridgeState.ReservationRejected:
                    notifyingUnableToJoin = true;
                    break;
                default:
                    break;
            }
        }

        private IEnumerator LaunchSession()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
            StartCoroutine(startNewGame);
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);
            Multiplayer.Main.StartMultiplayer();
        }

        private GUISkin GetGUISkin()
        {
            return GUISkinUtils.RegisterDerivedOnce("menus.server", s =>
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
                s.label.fixedWidth = 80; //change this when adding new labels that need more space.

                s.button.fontSize = 14;
                s.button.stretchHeight = true;
            });
        }

        private void BuildJoinServerDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        StartCoroutine(NegotiateSession(ServerIp));
                        joiningServer = false;
                        break;
                    case KeyCode.Escape:
                        joiningServer = false;
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin(), () => 
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
						StartCoroutine(NegotiateSession(ServerIp));
						joiningServer = false;
					}

					if (GUILayout.Button("Cancel"))
					{
						joiningServer = false;
					}
				}
            });

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }

        private void BuildUnableToJoinDialog(int windowId)
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

            SetGUIStyle();
            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box"))
            {
                using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"Rejection Reason: { Multiplayer.Logic.ClientBridge.ReservationRejectionReason.ToString() }");
                }

                if (GUILayout.Button("OK"))
                {
                    joiningServer = true;
                    notifyingUnableToJoin = false;
                }
            }
        }
    }
}
