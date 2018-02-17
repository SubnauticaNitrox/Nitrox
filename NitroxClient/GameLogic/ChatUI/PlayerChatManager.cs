using NitroxClient.MonoBehaviours.Gui.Chat;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    // TODO: Naming? This class facilitates communication between the chat entry text box and the primary chat log. Manager doesn't really seem to describe that intent.
    public class PlayerChatManager
    {
        private static PlayerChatLog chatLog;
        private static PlayerChatEntry chatEntry;

        public PlayerChatManager()
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

        public void ShowChat()
        {
            chatEntry.Show(this);
        }
    }
}
