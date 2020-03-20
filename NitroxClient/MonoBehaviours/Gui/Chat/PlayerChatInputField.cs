using System;
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

        public bool ChatEnabled
        {
            get
            {
                return chatEnabled;
            }
            set
            {
                // If another input has focus then do not enable chat
                if (FPSInputModule.current.lastGroup != null)
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
                // Remove "/" and split on arguments. TODO: Send as string and let server parse the command
                session.Send(new ServerCommand(chatMessage.Remove(0, 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
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
