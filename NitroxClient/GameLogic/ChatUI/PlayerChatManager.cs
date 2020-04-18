using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.Chat;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    public class PlayerChatManager
    {
        public PlayerChatManager(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
            Player.main.StartCoroutine(LoadChatLogAsset());
        }

        private PlayerChat playerChat;
        public Transform PlayerChaTransform => playerChat.transform;
        private readonly IMultiplayerSession multiplayerSession;
        private const char SERVER_COMMAND_PREFIX = '/';

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
            playerChat.WriteLogEntry(multiplayerSession.AuthenticationContext.Username, playerChat.InputText, multiplayerSession.PlayerSettings.PlayerColor);
            playerChat.InputText = "";
            playerChat.Select();
        }

        private IEnumerator LoadChatLogAsset()
        {
            yield return AssetBundleLoader.LoadUIAsset("chatlog", "PlayerChatCanvas", true, playerChatGameObject =>
            {
                playerChat = playerChatGameObject.AddComponent<PlayerChat>();
            });

            yield return playerChat.SetupChatComponents();
        }

        public static void LoadChatKeyHint() => Player.main.StartCoroutine(AssetBundleLoader.LoadUIAsset("chatkeyhint", "ChatKeyCanvas"));
    }
}
