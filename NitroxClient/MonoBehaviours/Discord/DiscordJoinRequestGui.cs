using System.Collections;
using DiscordGameSDKWrapper;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.MonoBehaviours.Discord;

public class DiscordJoinRequestGui : uGUI_InputGroup
{
    private const int EXPIRE_TIME = 45;

    private static DiscordJoinRequestGui instance;
    private static User user;

    private static Image profilePicture;
    private static GameObject pressToFocus;
    private static GameObject pressButtons;

    public static IEnumerator SpawnGui(User requestingUser)
    {
        user = requestingUser;

        yield return LoadUIAsset(NitroxAssetBundle.DISCORD_JOIN_REQUEST, false);

        GameObject guiGameObject = (GameObject)NitroxAssetBundle.DISCORD_JOIN_REQUEST.LoadedAssets[0];

        instance = guiGameObject.AddComponent<DiscordJoinRequestGui>();

        profilePicture = guiGameObject.FindChild("ProfilePicture").GetComponent<Image>();

        pressToFocus = guiGameObject.FindChild("PressToFocus");
        Text[] texts = pressToFocus.GetComponentsInChildren<Text>();
        texts[0].text = Language.main.Get("Nitrox_DiscordPressToFocus");
        texts[3].text = GameInput.GetBinding(GameInput.Device.Keyboard, (GameInput.Button)46, GameInput.BindingSet.Primary);
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
        DestroyImmediate(gameObject);
    }

    public static void Select()
    {
        if (instance)
        {
            pressToFocus.SetActive(false);
            pressButtons.SetActive(true);
            instance.Select(false);
        }
    }

    private static IEnumerator LoadAvatar(long id, string avatarID)
    {
        UnityWebRequest avatarUrl = UnityWebRequestTexture.GetTexture($"https://cdn.discordapp.com/avatars/{id}/{avatarID}.png");
        yield return avatarUrl.SendWebRequest();

        Texture2D avatar = ((DownloadHandlerTexture)avatarUrl.downloadHandler).texture;

        if (!avatar || avatar.height < 64)
        {
            UnityWebRequest standardAvatarUrl = UnityWebRequestTexture.GetTexture("https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png");
            yield return standardAvatarUrl.SendWebRequest();

            avatar = ((DownloadHandlerTexture)standardAvatarUrl.downloadHandler).texture;
        }

        profilePicture.sprite = Sprite.Create(avatar, new UnityEngine.Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator OnRequestExpired()
    {
        yield return new WaitForSeconds(EXPIRE_TIME);
        CloseWindow(ActivityJoinRequestReply.Ignore);
    }
}
