using System.Collections;
using NitroxClient.Helpers.DiscordGameSDK;
using NitroxClient.MonoBehaviours.DiscordRP;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using UnityEngine.Networking;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public class DiscordJoinRequestGui : MonoBehaviour
{
    public User User;

    private const int EXPIRE_TIME = 30;
    private Rect windowRect = new(Screen.width - 260, 10, 250, 125);
    private Texture2D avatar;

    public void Start()
    {
        StartCoroutine(LoadAvatar(User.Id, User.Avatar));
        StartCoroutine(OnRequestExpired());
    }

    public void OnGUI()
    {
        windowRect = GUILayout.Window(0, windowRect, DrawDiscordRequestWindow, "Server join request from Discord");
    }

    private static GUISkin GetGUISkin()
    {
        GUISkin guiSkin = GUI.skin;
        guiSkin.label.fontSize = 14;
        guiSkin.label.alignment = TextAnchor.MiddleLeft;
        guiSkin.label.stretchHeight = true;
        guiSkin.label.fixedWidth = 100;
        guiSkin.label.fixedHeight = 70;
        guiSkin.label.margin = new RectOffset(7, 7, 2, 2);
        guiSkin.label.richText = true;
        guiSkin.button.fontSize = 14;
        return guiSkin;
    }

    private void DrawDiscordRequestWindow(int windowID)
    {
        GUISkinUtils.RenderWithSkin(GetGUISkin(), () =>
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(avatar);
                    GUILayout.Label($"<b>{User.Username}</b> wants to join your server.");
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Accept"))
                    {
                        CloseWindow(ActivityJoinRequestReply.Yes);
                    }

                    if (GUILayout.Button("Deny"))
                    {
                        CloseWindow(ActivityJoinRequestReply.No);
                    }
                }
            }
        });
    }

    private void CloseWindow(ActivityJoinRequestReply reply)
    {
        DiscordClient.RespondJoinRequest(User.Id, reply);
        Destroy(this);
    }

    private IEnumerator LoadAvatar(long id, string avatarID)
    {
        UnityWebRequest avatarUrl = UnityWebRequestTexture.GetTexture($"https://cdn.discordapp.com/avatars/{id}/{avatarID}.png?size=64");
        yield return avatarUrl.SendWebRequest();

        avatar = ((DownloadHandlerTexture)avatarUrl.downloadHandler).texture;

        if (avatar == null || avatar.height < 64)
        {
            UnityWebRequest standardAvatarUrl = UnityWebRequestTexture.GetTexture("https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png");
            yield return standardAvatarUrl.SendWebRequest();

            avatar = ((DownloadHandlerTexture)standardAvatarUrl.downloadHandler).texture;
        }

        TextureScaler.Scale(avatar, 64, 64);
        yield return null;
    }

    private IEnumerator OnRequestExpired()
    {
        yield return new WaitForSeconds(EXPIRE_TIME);
        CloseWindow(ActivityJoinRequestReply.Ignore);
    }
}
