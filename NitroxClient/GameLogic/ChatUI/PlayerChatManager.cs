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

        public void ShowChat() => Player.main.StartCoroutine(ShowChatAsync());
        private IEnumerator ShowChatAsync()
        {
            while (PlayerChat.IsLoading)
            {
                yield return null;
            }
            playerChat.Show();
        }

        public void HideChat() => Player.main.StartCoroutine(HideChatAsync());
        private IEnumerator HideChatAsync()
        {
            while (PlayerChat.IsLoading)
            {
                yield return null;
            }
            playerChat.Deselect();
            playerChat.Hide();
        }

        public void SelectChat() => Player.main.StartCoroutine(SelectChatAsync());
        private IEnumerator SelectChatAsync()
        {
            while (PlayerChat.IsLoading)
            {
                yield return null;
            }
            playerChat.Show();
            playerChat.Select();
        }

        public void AddMessage(string playerName, string message, Color color) => Player.main.StartCoroutine(AddMessageAsync(playerName, message, color));
        private IEnumerator AddMessageAsync(string playerName, string message, Color color)
        {
            while (PlayerChat.IsLoading)
            {
                yield return null;
            }

            playerChat.WriteLogEntry(playerName, message, color);
        }

        public void SendMessage()
        {
            if (playerChat.inputText.Trim() != "")
            {
                multiplayerSession.Send(new ChatMessage(multiplayerSession.Reservation.PlayerId, playerChat.inputText));
                playerChat.WriteLogEntry(multiplayerSession.AuthenticationContext.Username, playerChat.inputText, multiplayerSession.PlayerSettings.PlayerColor);
                playerChat.inputText = "";
            }
            playerChat.Select();
        }

        private IEnumerator LoadChatLogAsset()
        {
            CoroutineWithData coroutineWD = new CoroutineWithData(Player.main, AssetBundleLoader.LoadUIAsset("chatlog", "PlayerChatCanvas"));
            yield return coroutineWD.Coroutine;

            playerChat = ((GameObject)coroutineWD.Result).gameObject.AddComponent<PlayerChat>();
            yield return playerChat.SetupChatComponents();
        }

        public static void LoadChatKeyHint() => Player.main.StartCoroutine(AssetBundleLoader.LoadUIAsset("chatkeyhint", "ChatKeyCanvas"));
    }
}
