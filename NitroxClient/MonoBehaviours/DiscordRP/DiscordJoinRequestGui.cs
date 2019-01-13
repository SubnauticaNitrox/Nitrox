using System.Collections;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class DiscordJoinRequestGui : MonoBehaviour
    {
        public DiscordController DiscordRp;
        public DiscordRpc.DiscordUser Request;

        Rect discordWindowRect = new Rect(Screen.width - 260, 10, 250, 125);
        Texture2D avatar;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            DiscordRp = gameObject.GetComponent<DiscordController>();
            StartCoroutine(LoadAvatar(Request.userId, Request.avatar));
            StartCoroutine(OnRequestExpired());
        }

        public void OnGUI()
        {
            discordWindowRect = GUILayout.Window(0, discordWindowRect, DoDiscordWindow, "Server join request from Discord");
        }

        private GUISkin SetGUIStyle()
        {
            GUISkin skin = GUI.skin;
            skin.label.fontSize = 14;
            skin.label.alignment = TextAnchor.MiddleLeft;
            skin.label.stretchHeight = true;
            skin.label.fixedWidth = 100;
            skin.label.fixedHeight = 70;
            skin.label.margin = new RectOffset(7, 7, 2, 2);
            skin.label.richText = true;

            skin.button.fontSize = 14;
            return skin;
        }

        private void DoDiscordWindow(int windowId)
        {
            GUISkinUtils.RenderWithSkin(SetGUIStyle(), () =>
            {
                using (GUILayout.VerticalScope v = new GUILayout.VerticalScope("Box"))
                {
                    using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(avatar);
                        GUILayout.Label("<b>" + Request.username + "</b> wants to join your server.");
                    }
                    using (GUILayout.HorizontalScope b = new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Accept"))
                        {
                            CloseWindow(1);
                        }

                        if (GUILayout.Button("Deny"))
                        {
                            CloseWindow(0);
                        }
                    }
                }
            });
        }

        private void CloseWindow(int respond)
        {
            DiscordRp.RespondLastJoinRequest(respond);
            DiscordRp.ShowingWindow = false;
            Destroy(this);
        }

        private IEnumerator LoadAvatar(string id, string avatar_id)
        {
            WWW avatarURL = new WWW("https://cdn.discordapp.com/avatars/" + id + "/" + avatar_id + ".png");
            yield return avatarURL;

            if (avatarURL.texture != null && avatarURL.texture.height >= 128)
            {
                avatar = avatarURL.texture;
            }
            else
            {
                WWW standardAvatarURL = new WWW("https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png");
                yield return standardAvatarURL;

                avatar = standardAvatarURL.texture;
            }
            TextureScaler.Scale(avatar, 64, 64);

            yield return null;
        }

        private IEnumerator OnRequestExpired()
        {
            yield return new WaitForSeconds(30);
            CloseWindow(2);
        }
    }
}
