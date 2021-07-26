using System.Collections;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.Networking;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class DiscordJoinRequestGui : MonoBehaviour
    {
        public DiscordRpc.DiscordUser Request;

        private const int EXPIRE_TIME = 30;
        private Rect windowRect = new Rect(Screen.width - 260, 10, 250, 125);
        private Texture2D avatar;
        private GUISkin guiSkin;

        public void Start()
        {
            SetupGUISkin();
            StartCoroutine(LoadAvatar(Request.userId, Request.avatar));
            StartCoroutine(OnRequestExpired());
        }

        public void OnGUI()
        {
            windowRect = GUILayout.Window(0, windowRect, DrawDiscordRequestWindow, "Server join request from Discord");
        }

        private void SetupGUISkin()
        {
            guiSkin = GUI.skin;
            guiSkin.label.fontSize = 14;
            guiSkin.label.alignment = TextAnchor.MiddleLeft;
            guiSkin.label.stretchHeight = true;
            guiSkin.label.fixedWidth = 100;
            guiSkin.label.fixedHeight = 70;
            guiSkin.label.margin = new RectOffset(7, 7, 2, 2);
            guiSkin.label.richText = true;
            guiSkin.button.fontSize = 14;
        }

        private void DrawDiscordRequestWindow(int windowID)
        {
            GUISkinUtils.RenderWithSkin(guiSkin, () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(avatar);
                        GUILayout.Label("<b>" + Request.username + "</b> wants to join your server.");
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Accept"))
                        {
                            CloseWindow(DiscordRpc.Reply.YES);
                        }

                        if (GUILayout.Button("Deny"))
                        {
                            CloseWindow(DiscordRpc.Reply.NO);
                        }
                    }
                }
            });
        }

        private void CloseWindow(DiscordRpc.Reply reply)
        {
            DiscordRPController.Main.RespondJoinRequest(Request.userId, reply);
            Destroy(this);
        }

        private IEnumerator LoadAvatar(string id, string avatarID)
        {
            UnityWebRequest avatarURL = UnityWebRequestTexture.GetTexture($"https://cdn.discordapp.com/avatars/{id}/{avatarID}.png");

            yield return avatarURL.SendWebRequest();

            avatar = ((DownloadHandlerTexture)avatarURL.downloadHandler).texture;

            if (avatar == null || avatar.height < 128)
            {
                UnityWebRequest standardAvatarURL = UnityWebRequestTexture.GetTexture("https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png");
                yield return standardAvatarURL.SendWebRequest();

                avatar = ((DownloadHandlerTexture)standardAvatarURL.downloadHandler).texture;
            }

            TextureScaler.Scale(avatar, 64, 64);

            yield return null;
        }

        private IEnumerator OnRequestExpired()
        {
            yield return new WaitForSeconds(EXPIRE_TIME);
            CloseWindow(DiscordRpc.Reply.IGNORE);
        }
    }
}
