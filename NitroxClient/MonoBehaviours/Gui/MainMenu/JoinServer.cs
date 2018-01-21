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
            joinServerWindowRect = GUILayout.Window(0, joinServerWindowRect, DoJoinServerWindow, "Join server");
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

        private void SetGUIStyle()
        {
            GUI.skin.textField.fontSize = 14;
            GUI.skin.textField.richText = false;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.wordWrap = true;
            GUI.skin.textField.stretchHeight = true;
            GUI.skin.textField.padding = new RectOffset(10, 10, 5, 5);

            GUI.skin.label.fontSize = 14;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.skin.label.stretchHeight = true;
            GUI.skin.label.fixedWidth = 80; //change this when adding new labels that need more space.

            GUI.skin.button.fontSize = 14;
            GUI.skin.button.stretchHeight = true;
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

            SetGUIStyle();
            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box"))
            {
                using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope())
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

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }
    }
}
