using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Gui.Chat;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.GameLogic.ChatUI
{
    public class PlayerChatManager
    {
        private readonly IMultiplayerSession multiplayerSession;

        private const char SERVER_COMMAND_PREFIX = '/';

        public bool IsChatSelected
        {
            get => PlayerChat.IsReady && playerChat.selected;
        }

        public PlayerChatManager(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;

            if (NitroxEnvironment.IsNormal) //Testing would fail because it's trying to access runtime MonoBehaviours.
            {
                CoroutineHost.StartCoroutine(LoadChatLogAsset());
            }
        }

        private PlayerChat playerChat;
        private GameObject chatKeyHint;
        public Transform PlayerChaTransform => playerChat.transform;

        public void ShowChat() => Player.main.StartCoroutine(ShowChatAsync());
        private IEnumerator ShowChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Show();
        }

        public void HideChat() => Player.main.StartCoroutine(HideChatAsync());
        private IEnumerator HideChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Deselect();
            playerChat.Hide();
        }

        public void SelectChat() => Player.main.StartCoroutine(SelectChatAsync());
        private IEnumerator SelectChatAsync()
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.Show();
            playerChat.Select();

            if (!NitroxPrefs.ChatUsed.Value)
            {
                DisableChatKeyHint();
            }
        }

        public void AddMessage(string playerName, string message, Color color) => Player.main.StartCoroutine(AddMessageAsync(playerName, message, color));
        private IEnumerator AddMessageAsync(string playerName, string message, Color color)
        {
            yield return new WaitUntil(() => PlayerChat.IsReady);
            playerChat.WriteLogEntry(playerName, message, color);
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
                multiplayerSession.Send(new ServerCommand(trimmedInput.Substring(1)));
                playerChat.InputText = "";
                playerChat.Select();
                return;
            }

            // We shouldn't add the message to the local chat instantly but instead let the server tell us if this message is added or not
            multiplayerSession.Send(new ChatMessage(multiplayerSession.Reservation.PlayerId, trimmedInput));
            playerChat.InputText = "";
            playerChat.Select();
        }

        private IEnumerator LoadChatLogAsset()
        {
            yield return LoadUIAsset(NitroxAssetBundle.CHAT_LOG, true);
            
            GameObject playerChatGameObject = (GameObject)NitroxAssetBundle.CHAT_LOG.LoadedAssets[0];
            playerChat = playerChatGameObject.AddComponent<PlayerChat>();
            
            yield return playerChat.SetupChatComponents();
        }

        public IEnumerator LoadChatKeyHint()
        {
            if (!NitroxPrefs.ChatUsed.Value)
            {
                yield return LoadUIAsset(NitroxAssetBundle.CHAT_KEY_HINT, false);
                chatKeyHint = NitroxAssetBundle.CHAT_KEY_HINT.LoadedAssets[0] as GameObject;
            }
        }

        private void DisableChatKeyHint()
        {
            chatKeyHint.GetComponentInChildren<Text>().CrossFadeAlpha(0, 1, false);
            chatKeyHint.GetComponentInChildren<Image>().CrossFadeAlpha(0, 1, false);
            NitroxPrefs.ChatUsed.Value = true;
        }
    }
}
