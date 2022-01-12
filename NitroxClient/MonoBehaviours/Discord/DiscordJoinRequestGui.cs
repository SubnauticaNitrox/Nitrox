using System.Collections;
using NitroxClient.Helpers.DiscordGameSDK;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Discord;

public class DiscordJoinRequestGui : uGUI_InputGroup
{
    private const int EXPIRE_TIME = 30;

    private static DiscordJoinRequestGui instance;
    private User user;

    private static Image profilePicture;

    private static GameObject pressToFocus;
    private static GameObject pressButtons;

    public static IEnumerator SpawnGui(User user)
    {
        yield return AssetBundleLoader.LoadUIAsset("discordjoinrequest", false, guiGameObject =>
        {
            instance = guiGameObject.AddComponent<DiscordJoinRequestGui>();
            instance.user = user;

            profilePicture = guiGameObject.FindChild("ProfilePicture").GetComponent<Image>();

            pressToFocus = guiGameObject.FindChild("PressToFocus");
            pressToFocus.FindChild("PressToFocusLabel").GetComponent<Text>().text = Language.main.Get("Nitrox_DiscordPressToFocus");
            pressToFocus.SetActive(true);

            pressButtons = guiGameObject.FindChild("PressButtons");
            pressButtons.SetActive(false);

            Text[] buttonTexts = pressButtons.GetComponentsInChildren<Text>(true);
            buttonTexts[0].text = Language.main.Get("Nitrox_DiscordAccept");
            buttonTexts[1].text = Language.main.Get("Nitrox_DiscordDecline");

            Button[] buttons = pressButtons.GetComponentsInChildren<Button>(true);
            buttons[0].onClick.AddListener(instance.AcceptInvite);
            buttons[1].onClick.AddListener(instance.DeclineInvite);

            Text[] userTexts = guiGameObject.FindChild("UpperText").GetComponentsInChildren<Text>();
            userTexts[0].text = $"{user.Username}#{user.Discriminator}";
            userTexts[1].text = Language.main.Get("Nitrox_DiscordRequestText");
        });

        yield return LoadAvatar(user.Id, user.Avatar);
    }

    private void Start()
    {
        StartCoroutine(OnRequestExpired());
    }

    private void AcceptInvite() => CloseWindow(ActivityJoinRequestReply.Yes);
    private void DeclineInvite() => CloseWindow(ActivityJoinRequestReply.No);

    private void CloseWindow(ActivityJoinRequestReply reply)
    {
        DiscordClient.RespondJoinRequest(user.Id, reply);
        Destroy(this);
    }

    public static void Select()
    {
        if (instance)
        {
            instance.Select(false);
        }
    }

    private static IEnumerator LoadAvatar(long id, string avatarID)
    {
        UnityWebRequest avatarUrl = UnityWebRequestTexture.GetTexture($"https://cdn.discordapp.com/avatars/{id}/{avatarID}.png");
        yield return avatarUrl.SendWebRequest();

        Texture2D avatar = ((DownloadHandlerTexture)avatarUrl.downloadHandler).texture;

        if (avatar == null || avatar.height < 64)
        {
            UnityWebRequest standardAvatarUrl = UnityWebRequestTexture.GetTexture("https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png");
            yield return standardAvatarUrl.SendWebRequest();

            avatar = ((DownloadHandlerTexture)standardAvatarUrl.downloadHandler).texture;
        }

        profilePicture.sprite = Sprite.Create(avatar, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator OnRequestExpired()
    {
        yield return new WaitForSeconds(EXPIRE_TIME);
        CloseWindow(ActivityJoinRequestReply.Ignore);
    }
}
