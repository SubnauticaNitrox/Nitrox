using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    /// <summary>
    ///     A text box for the player to enter and say something in-game.
    /// </summary>
    internal class PlayerChatInputField : MonoBehaviour
    {
        private const int CHAR_LIMIT = 80;
        private const int INPUT_WIDTH = 300;
        private const int INPUT_HEIGHT = 35;
        private const int INPUT_MARGIN = 15;
        private const string GUI_CHAT_NAME = "ChatInput";
        private const char SERVER_COMMAND_PREFIX = '/';
        private bool chatEnabled;

        private string chatMessage = "";

        /// <summary>
        ///     Uses UWE input field logic to prevent player from moving while typing.
        /// </summary>
        private uGUI_InputGroup inputGroup;

        private IMultiplayerSession session;

        /// <summary>
        ///     Gets or sets the chat enabled. Can fail to enable chat if another input field in-game already has focus.
        /// </summary>
        public bool ChatEnabled
        {
            get
            {
                return chatEnabled;
            }
            set
            {
                if (!CanEnable())
                {
                    return;
                }
                // Reuse UWE logic to lock player in-place.
                if (value)
                {
                    inputGroup.OnSelect(true);
                }
                else
                {
                    inputGroup.OnDeselect();
                }

                chatEnabled = value;
            }
        }

        public PlayerChat Manager { get; set; }

        /// <summary>
        ///     Gets true if no other (UWE) input field has focus right now.`
        /// </summary>
        /// <returns>True if the chat input field can be enabled without interfering with another input field in-game.</returns>
        public bool CanEnable()
        {
            return FPSInputModule.current.lastGroup == null;
        }

        public void Awake()
        {
            session = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            inputGroup = gameObject.AddComponent<uGUI_InputGroup>();
        }

        public void OnGUI()
        {
            if (!ChatEnabled || Manager == null)
            {
                return;
            }

            SetGUIStyle();
            GUI.SetNextControlName(GUI_CHAT_NAME);
            chatMessage = GUI.TextField(new Rect(INPUT_MARGIN, Screen.height - INPUT_HEIGHT - INPUT_MARGIN, INPUT_WIDTH, INPUT_HEIGHT), chatMessage, CHAR_LIMIT);
            GUI.FocusControl(GUI_CHAT_NAME);

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                        SendMessage();
                        ChatEnabled = false;
                        break;
                    case KeyCode.Escape:
                        Manager.HideLog();
                        ChatEnabled = false;
                        break;
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
            if (Manager == null || string.IsNullOrWhiteSpace(chatMessage))
            {
                chatMessage = "";
                return;
            }

            if (chatMessage[0] == SERVER_COMMAND_PREFIX)
            {
                session.Send(new ServerCommand(chatMessage.Remove(0, 1)));
            }
            else
            {
                session.Send(new ChatMessage(session.Reservation.PlayerId, chatMessage));
                Manager.AddMessage("Me", chatMessage, session.PlayerSettings.PlayerColor);
            }
            chatMessage = "";
        }
    }
}
