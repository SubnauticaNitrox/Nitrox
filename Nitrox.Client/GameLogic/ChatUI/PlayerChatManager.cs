using System.Collections;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.MonoBehaviours.Gui.Chat;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using UnityEngine.UI;

namespace Nitrox.Client.GameLogic.ChatUI
{
    public class PlayerChatManager
    {
        private readonly IMultiplayerSession multiplayerSession;

        private const char SERVER_COMMAND_PREFIX = '/';
        private const string CHAT_LOG_ASSET = "chatlog";
        private const string CHAT_KEY_HINT_ASSET = "chatkeyhint";

        public PlayerChatManager(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
            Player.main.StartCoroutine(LoadChatLogAsset());
        }

        private PlayerChat playerChat;
        private GameObject chatKeyHint;
        public Transform PlayerChaTransform => playerChat.transform;

        private bool chatUsed
        {
            get
            {
                if (PlayerPrefs.HasKey("Nitrox.chatUsed"))
                {
                    return PlayerPrefs.GetInt("Nitrox.chatUsed") == 1;
                }

                return false;
            }
            set
            {
                PlayerPrefs.SetInt("Nitrox.chatUsed", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

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

            if (!chatUsed)
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

            // Chat message
            multiplayerSession.Send(new ChatMessage(multiplayerSession.Reservation.PlayerId, trimmedInput));
            playerChat.WriteLogEntry(multiplayerSession.AuthenticationContext.Username, playerChat.InputText, multiplayerSession.PlayerSettings.PlayerColor.ToUnity());
            playerChat.InputText = "";
            playerChat.Select();
        }

        private IEnumerator LoadChatLogAsset()
        {
            yield return AssetBundleLoader.LoadUIAsset(CHAT_LOG_ASSET, "PlayerChatCanvas", true, playerChatGameObject =>
            {
                playerChat = playerChatGameObject.AddComponent<PlayerChat>();
            });

            yield return playerChat.SetupChatComponents();
        }

        public void LoadChatKeyHint()
        {
            if (!chatUsed)
            {
                Player.main.StartCoroutine(AssetBundleLoader.LoadUIAsset(CHAT_KEY_HINT_ASSET, "ChatKeyCanvas", false, chatKeyHintGameObject =>
                {
                    chatKeyHint = chatKeyHintGameObject;
                }));
            }
        }

        //TODO: Has to be reworked if the config API for Nitrox.Client is finished.
        private void DisableChatKeyHint()
        {
            chatKeyHint.GetComponentInChildren<Text>().CrossFadeAlpha(0, 1, false);
            chatKeyHint.GetComponentInChildren<Image>().CrossFadeAlpha(0, 1, false);
            chatUsed = true;
        }
    }
}
