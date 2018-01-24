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
        string username = "username";
        bool showingUsername = false;
        bool shouldFocus;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            showingUsername = true;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (!showingUsername)
            {
                return;
            }
            joinServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, DoJoinServerWindow, "Join server");
        }

        public IEnumerator JoinServerWait(string serverIp)
        {
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
            StartCoroutine(startNewGame);
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);
            Multiplayer.Main.StartMultiplayer(serverIp, username);
            Destroy(gameObject);
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

        private void DoJoinServerWindow(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        StartCoroutine(JoinServerWait(ServerIp));
                        showingUsername = false;
                        break;
                    case KeyCode.Escape:
                        showingUsername = false;
                        break;
                }
            }

            GUISkinUtils.SwitchSkin(GetGUISkin(), () => 
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
                        StartCoroutine(JoinServerWait(ServerIp));
                        showingUsername = false;
                    }

                    if (GUILayout.Button("Cancel"))
                    {
                        showingUsername = false;
                    }
                }
            });

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }
    }
}
