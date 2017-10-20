using NitroxClient.MonoBehaviours.Gui.Chat;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    public class PlayerChatManager
    {
        private static PlayerChat chat;
        private static PlayerChatEntry chatEntry;

        public PlayerChatManager()
        {
            if (chat == null)
            {
                chat = new GameObject().AddComponent<PlayerChat>();
            }

            if (chatEntry == null)
            {
                chatEntry = new GameObject().AddComponent<PlayerChatEntry>();
            }
        }

        public void WriteMessage(string message, Color color)
        {
            chat.WriteMessage(message, color);
        }

        public void ShowChat()
        {
            chatEntry.Show(this);
        }
    }
}
