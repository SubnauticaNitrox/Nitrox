using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Gui.Chat;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.UI;

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
                Player.main.StartCoroutine(LoadChatLogAsset());
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
            yield return AssetBundleLoader.LoadUIAsset("chatlog", true, playerChatGameObject =>
            {
                playerChat = playerChatGameObject.AddComponent<PlayerChat>();
            });

            yield return playerChat.SetupChatComponents();
        }

        public void LoadChatKeyHint()
        {
            if (!NitroxPrefs.ChatUsed.Value)
            {
                Player.main.StartCoroutine(AssetBundleLoader.LoadUIAsset("chatkeyhint", false, chatKeyHintGameObject =>
                {
                    chatKeyHint = chatKeyHintGameObject;
                }));
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
