using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    class PlayerChatEntry : MonoBehaviour
    {
        private const int CHAR_LIMIT = 80;
        private const int INPUT_WIDTH = 300;
        private const int INPUT_HEIGHT = 35;
        private const int INPUT_MARGIN = 15;
        private const string GUI_CHAT_NAME = "ChatInput";

        private bool chatEnabled = false;
        private string chatMessage = "";
        private PlayerChatManager chatManager;
        private GameLogic.Chat chatBroadcaster;

        public void Awake()
        {
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

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    SendMessage();
                    Hide();
                }

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                {
                    Hide();
                }
            }
        }

        private void SetGUIStyle()
        {
            GUI.skin.textField.fontSize = 16;
            GUI.skin.textField.richText = false;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
        }

        private void SendMessage()
        {
            if (chatManager != null && chatMessage.Length > 0)
            {
                chatBroadcaster.SendChatMessage(chatMessage);
                chatManager.WriteMessage("Me: " + chatMessage);
            }
        }

        public void Show(PlayerChatManager currentChatManager)
        {
            chatManager = currentChatManager;
            chatEnabled = true;
        }

        public void Hide()
        {
            chatEnabled = false;
            chatMessage = "";
        }
    }
}
