using System.Collections;
using NitroxClient.MonoBehaviours.DiscordRP;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class AcceptRequest : MonoBehaviour
    {
        public DiscordController DiscordRp;
        Rect discordWindowRect = new Rect(Screen.width / 2 - 250, 100, 500, 250);
        public DiscordRpc.JoinRequest Request;
        bool showingWindow = false;
        Image avatar;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            DiscordRp = gameObject.GetComponent<DiscordController>();
            showingWindow = true;
        }

        public void OnGUI()
        {
            if (!showingWindow)
            {
                return;
            }
            discordWindowRect = GUILayout.Window(0, discordWindowRect, DoDiscordWindow, "Server join request from Discord");
        }

        private void SetGUIStyle()
        {
            GUI.skin.textArea.fontSize = 19;
            GUI.skin.textArea.richText = false;
            GUI.skin.textArea.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textArea.wordWrap = true;
            GUI.skin.textArea.stretchHeight = true;
            GUI.skin.textArea.padding = new RectOffset(10, 10, 5, 5);

            GUI.skin.label.fontSize = 14;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.skin.label.stretchHeight = true;
            GUI.skin.label.fixedWidth = 80; //change this when adding new labels that need more space.

            GUI.skin.button.fontSize = 14;
            GUI.skin.button.stretchHeight = true;
        }

        private void DoDiscordWindow(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Escape:
                        DiscordRp.RespondLastJoinRequest(0);
                        showingWindow = false;
                        break;
                }
            }

            SetGUIStyle();
            using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box"))
            {
                using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope())
                {
                    avatar = new GameObject().AddComponent<Image>();
                    avatar.gameObject.transform.SetParent(gameObject.transform, false);
                    avatar.rectTransform.sizeDelta = new Vector2(500, 500);


                    //StartCoroutine(RealLoadImage("137677160327413760", "a_71155f3903aed5762ab9b535824f1ac1"));
                    //    GUILayout.TextField("Player " + lastJoinRequest.username + " wants to join your server.");
                    GUILayout.TextArea("Player Tjimen wants to join your server.");
                }
                using (GUILayout.HorizontalScope b = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Accept"))
                    {
                        DiscordRp.RespondLastJoinRequest(1);
                        showingWindow = false;
                    }

                    if (GUILayout.Button("Deny"))
                    {
                        DiscordRp.RespondLastJoinRequest(0);
                        showingWindow = false;
                    }
                }
            }
        }

        private IEnumerator RealLoadImage(string id, string avatar_id)
        {
            string url = "https://cdn.discordapp.com/avatars/" + id + "/" + avatar_id + ".png";
            WWW imageURLWWW = new WWW(url);
            yield return imageURLWWW;

            if (imageURLWWW.texture != null)
            {
                Sprite sprite = new Sprite();
                sprite = Sprite.Create(imageURLWWW.texture, new Rect(0, 0, 128, 128), Vector2.zero);

                avatar.sprite = sprite;
            }

            yield return null;
        }
    }
}
