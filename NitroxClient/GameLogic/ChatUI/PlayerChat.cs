using NitroxClient.MonoBehaviours.Gui.Chat;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    public class PlayerChat
    {
        /// <summary>
        ///     The Unity object that holds a record of what was said in-game.
        /// </summary>
        private static PlayerChatLog chatLog;

        /// <summary>
        ///     A text box where the player can enter something to add to the chat log.
        /// </summary>
        private static PlayerChatInputField inputField;

        public PlayerChat()
        {
            if (chatLog == null)
            {
                chatLog = new GameObject().AddComponent<PlayerChatLog>();
            }

            if (inputField == null)
            {
                inputField = new GameObject().AddComponent<PlayerChatInputField>();
                inputField.Manager = this;
            }
        }

        public void AddMessage(string playerName, string message, Color color)
        {
            ChatLogEntry entry = new ChatLogEntry(playerName, message, color);
            chatLog.WriteEntry(entry);
        }

        public void ShowLog()
        {
            chatLog.Show();
        }

        public void HideLog()
        {
            chatLog.Hide();
        }

        public void ShowChat()
        {
            inputField.ChatEnabled = true;
        }
    }
}
