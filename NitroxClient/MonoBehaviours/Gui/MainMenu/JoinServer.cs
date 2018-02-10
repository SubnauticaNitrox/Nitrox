using NitroxClient.Unity.Helper;
using System.Collections;
using NitroxModel.Helper;
using UnityEngine;
using NitroxModel.Logger;
using NitroxModel;
using System.ComponentModel;
using NitroxModel.PlayerSlot;

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
                joinServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, RenderJoinServerDialog, "Join server");
            }

            if (notifyingUnableToJoin)
            {
                unableToJoinWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, RenderUnableToJoinDialog, "Unable to Join Session");
            }
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

        private void RenderJoinServerDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        StartMultiplayerClient();
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
                        StartMultiplayerClient();
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

        private void StartMultiplayerClient()
        {
            Multiplayer.Main.SetUserData(ServerIp, username);
            Multiplayer.Main.enabled = true;
            joiningServer = false;
        }

        private void StopMultiplayerClient()
        {
            Multiplayer.Main.enabled = false;
            joiningServer = false;
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
                        PlayerSlotReservationState reservationState = Multiplayer.Logic.ClientBridge.ReservationState;
                        string reservationStateDescription = reservationState.Describe();

                        GUILayout.Label(reservationStateDescription);
                    }

                    if (GUILayout.Button("OK"))
                    {
                        joiningServer = true;
                        notifyingUnableToJoin = false;
                    }
                }
            });
        }
    }
}
