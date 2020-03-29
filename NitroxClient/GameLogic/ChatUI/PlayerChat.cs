using System;
using NitroxClient.MonoBehaviours.Gui.Chat;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    internal class PlayerChat
    {
        private static PlayerChat instance;
        private readonly Lazy<PlayerChatLog> chatLog;
        private readonly Lazy<PlayerChatInputField> inputField;

        /// <summary>
        ///     A text box where the player can enter something to add to the chat log.
        /// </summary>
        public PlayerChatInputField InputField => inputField.Value;

        /// <summary>
        ///     The Unity object that holds a record of what was said in-game.
        /// </summary>
        public PlayerChatLog ChatLog => chatLog.Value;

        public PlayerChat()
        {
            if (NitroxEnvironment.IsNormal && instance != null)
            {
                throw new Exception($"There must only be one {nameof(PlayerChat)} instance.");
            }
            instance = this;
            chatLog = new Lazy<PlayerChatLog>(() =>
            {
                PlayerChatLog chatlog = new GameObject().AddComponent<PlayerChatLog>();
                chatlog.Manager = this;
                return chatlog;
            });
            inputField = new Lazy<PlayerChatInputField>(() =>
            {
                PlayerChatInputField inputfield = new GameObject().AddComponent<PlayerChatInputField>();
                inputfield.Manager = this;
                return inputfield;
            });
        }

        public void AddMessage(string playerName, string message, Color color)
        {
            ChatLogEntry entry = new ChatLogEntry(playerName, message, color);
            ChatLog.WriteEntry(entry);
        }

        public void ShowLog()
        {
            ChatLog.Show();
        }

        public void HideLog()
        {
            ChatLog.Hide();
        }

        public void ShowChat()
        {
            ShowLog();
            InputField.ChatEnabled = true;
        }
    }
}
