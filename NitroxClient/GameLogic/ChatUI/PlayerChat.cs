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
        private static PlayerChatEntry chatEntry;

        public PlayerChat()
        {
            if (chatLog == null)
            {
                chatLog = new GameObject().AddComponent<PlayerChatLog>();
            }

            if (chatEntry == null)
            {
                chatEntry = new GameObject().AddComponent<PlayerChatEntry>();
            }
        }

        public void WriteChatLogEntry(ChatLogEntry chatLogEntry)
        {
            chatLog.WriteEntry(chatLogEntry);
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
            chatEntry.Show(this);
        }
    }
}
