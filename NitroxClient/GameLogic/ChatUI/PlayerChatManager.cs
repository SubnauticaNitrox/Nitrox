using NitroxClient.MonoBehaviours.Gui.Chat;
using System;
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

        public void WriteMessage(string message)
        {
            chat.WriteMessage(message);
        }

        public void ShowChat()
        {
            chatEntry.Show(this);
        }
    }
}
