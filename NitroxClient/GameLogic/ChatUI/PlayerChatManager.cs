using System.Collections;
using Nitrox.Model.Core;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Gui.Chat;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.GameLogic.ChatUI;

public class PlayerChatManager
{
    public delegate void PlayerChatDelegate(string message);

    public delegate void PlayerCommandDelegate(string message);

    private const char SERVER_COMMAND_PREFIX = '/';
    public static readonly PlayerChatManager Instance = new();
    private GameObject chatKeyHint;

    public PlayerChatDelegate OnPlayerChat;
    public PlayerCommandDelegate OnPlayerCommand;

    private PlayerChat playerChat;

    public bool IsChatSelected
    {
        get => PlayerChat.IsReady && playerChat.selected;
    }

    public Transform PlayerChatTransform => playerChat.transform;

    public PlayerChatManager()
    {
        if (NitroxEnvironment.IsNormal) //Testing would fail because it's trying to access runtime MonoBehaviours.
        {
            CoroutineHost.StartCoroutine(LoadChatLogAsset());
        }

        IEnumerator LoadChatLogAsset()
        {
            yield return LoadUIAsset(NitroxAssetBundle.CHAT_LOG, true);

            GameObject playerChatGameObject = (GameObject)NitroxAssetBundle.CHAT_LOG.LoadedAssets[0];
            playerChat = playerChatGameObject.AddComponent<PlayerChat>();

            yield return playerChat.SetupChatComponents();
        }
    }

    public void ShowChat()
    {
        CoroutineHost.StartCoroutine(ShowChatAsync());

        IEnumerator ShowChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Show();
        }
    }

    public void HideChat()
    {
        CoroutineHost.StartCoroutine(HideChatAsync());

        IEnumerator HideChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Deselect();
            playerChat.Hide();
        }
    }

    public void SelectChat()
    {
        CoroutineHost.StartCoroutine(SelectChatAsync());

        IEnumerator SelectChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Show();
            playerChat.Select();

            if (!NitroxPrefs.ChatUsed.Value)
            {
                DisableChatKeyHint();
            }
        }

        void DisableChatKeyHint()
        {
            chatKeyHint.GetComponentInChildren<Text>().CrossFadeAlpha(0, 1, false);
            chatKeyHint.GetComponentInChildren<Image>().CrossFadeAlpha(0, 1, false);
            NitroxPrefs.ChatUsed.Value = true;
        }
    }

    public void DeselectChat()
    {
        CoroutineHost.StartCoroutine(DeselectChatAsync());

        IEnumerator DeselectChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Deselect();
        }
    }

    public void AddMessage(string playerName, string message, Color color)
    {
        CoroutineHost.StartCoroutine(AddMessageAsync(playerName, message, color));

        IEnumerator AddMessageAsync(string playerName, string message, Color color)
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            yield return playerChat.WriteLogEntry(playerName, message, color);
        }
    }

    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(playerChat.InputText))
        {
            playerChat.Select();
            return;
        }

        string trimmedInput = playerChat.InputText.Trim();
        if (trimmedInput[0] == SERVER_COMMAND_PREFIX)
        {
            // Server command
            playerChat.InputText = "";
            playerChat.Select();
            OnPlayerCommand?.Invoke(trimmedInput.Substring(1));
            return;
        }

        // We shouldn't add the message to the local chat instantly but instead let the server tell us if this message is added or not
        playerChat.InputText = "";
        playerChat.Select();
        OnPlayerChat?.Invoke(trimmedInput);
    }

    public IEnumerator LoadChatKeyHint()
    {
        if (!NitroxPrefs.ChatUsed.Value)
        {
            yield return LoadUIAsset(NitroxAssetBundle.CHAT_KEY_HINT, false);
            chatKeyHint = NitroxAssetBundle.CHAT_KEY_HINT.LoadedAssets[0] as GameObject;
        }
    }
}
