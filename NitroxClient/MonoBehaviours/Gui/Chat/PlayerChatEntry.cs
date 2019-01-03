using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    /// <summary>
    ///     A text box for the player to enter and say something in-game.
    /// </summary>
    internal class PlayerChatEntry : MonoBehaviour
    {
        private const int CHAR_LIMIT = 80;
        private const int INPUT_WIDTH = 300;
        private const int INPUT_HEIGHT = 35;
        private const int INPUT_MARGIN = 15;
        private const string GUI_CHAT_NAME = "ChatInput";
        private PlayerChat chat;
        private GameLogic.Chat chatBroadcaster;

        private bool chatEnabled;
        private string chatMessage = "";
        private IMultiplayerSession multiplayerSession;

        public void Awake()
        {
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            chatBroadcaster = NitroxServiceLocator.LocateService<GameLogic.Chat>();
        }

        public void OnGUI()
        {
            if (chatEnabled)
            {
                SetGUIStyle();
                GUI.SetNextControlName(GUI_CHAT_NAME);
                chatMessage = GUI.TextField(new Rect(INPUT_MARGIN, Screen.height - INPUT_HEIGHT - INPUT_MARGIN, INPUT_WIDTH, INPUT_HEIGHT), chatMessage, CHAR_LIMIT);
                GUI.FocusControl(GUI_CHAT_NAME);
                chat.ShowLog();

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    chat.HideLog();
                    SendMessage();
                    Hide();
                }

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                {
                    chat.HideLog();
                    Hide();
                }
            }
        }

        public void Show(PlayerChat currentChat)
        {
            chat = currentChat;
            chatEnabled = true;
        }

        public void Hide()
        {
            chatEnabled = false;
            chatMessage = "";
        }

        private void SetGUIStyle()
        {
            GUI.skin.textField.fontSize = 16;
            GUI.skin.textField.richText = false;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
        }

        private void SendMessage()
        {
            if (chat != null && chatMessage.Length > 0)
            {
                chatBroadcaster.SendChatMessage(chatMessage);
                ChatLogEntry chatLogEntry = new ChatLogEntry("Me", chatMessage, multiplayerSession.PlayerSettings.PlayerColor);
                chat.WriteChatLogEntry(chatLogEntry);
            }
        }
    }
}
