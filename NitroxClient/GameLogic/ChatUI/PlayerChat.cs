using System;
using NitroxClient.MonoBehaviours.Gui.Chat;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    internal class PlayerChat
    {
        private static PlayerChat instance;

        /// <summary>
        ///     The Unity object that holds a record of what was said in-game.
        /// </summary>
        public readonly PlayerChatLog chatLog;

        /// <summary>
        ///     A text box where the player can enter something to add to the chat log.
        /// </summary>
        public readonly PlayerChatInputField inputField;

        public PlayerChat()
        {
            if (instance != null)
            {
                throw new Exception($"There must only be one {nameof(PlayerChat)} instance.");
            }
            instance = this;

            chatLog = new GameObject().AddComponent<PlayerChatLog>();
            chatLog.Manager = this;
            inputField = new GameObject().AddComponent<PlayerChatInputField>();
            inputField.Manager = this;
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
            ShowLog();
            inputField.ChatEnabled = true;
        }
    }
}
