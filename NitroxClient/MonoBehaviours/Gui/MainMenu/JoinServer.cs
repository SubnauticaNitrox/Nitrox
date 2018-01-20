using System.Collections;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        public string ServerIp = "";
        private Rect joinServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        string username = "username";
        bool showingUsername = false;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            showingUsername = true;
        }

        public void OnGUI()
        {
            if (!showingUsername)
            {
                return;
            }
            joinServerWindowRect = GUILayout.Window(0, joinServerWindowRect, DoJoinServerWindow, "Join server");
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

            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box", GUILayout.ExpandHeight(true)))
            {
                username = GUILayout.TextField(username, GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Join", GUILayout.ExpandHeight(true)))
                {
                    StartCoroutine(JoinServerWait(ServerIp));
                    showingUsername = false;
                }
                if (GUILayout.Button("Cancel", GUILayout.ExpandHeight(true)))
                {
                    showingUsername = false;
                }
            }
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
    }
}
