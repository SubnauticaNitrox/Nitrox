using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    /// <summary>
    ///     The Unity object that holds a record of what was said in-game.
    /// </summary>
    internal class PlayerChatLog : MonoBehaviour
    {
        private const int LINE_CHAR_LIMIT = 80;
        private const int MESSAGE_LIMIT = 6;
        private const float CHAT_VISIBILITY_TIME_LENGTH = 7f;

        private GameObject chatEntry;
        private GUIText chatText;
        private List<ChatLogEntry> entries;
        private Coroutine timer;

        /// <summary>
        ///     Takes a new chat message and displays that message along with MESSAGE_LIMIT-1 previous entries for
        ///     CHAT_VISIBILITY_TIME_LENGTH seconds
        /// </summary>
        /// <param name="chatLogEntry"></param>
        public void WriteEntry(ChatLogEntry chatLogEntry)
        {
            if (timer != null)
            {
                // cancel hiding chat entries because a new one was recently posted
                StopCoroutine(timer);
            }

            chatLogEntry.MessageText = SanitizeMessage(chatLogEntry.MessageText);
            AddChatMessage(chatLogEntry);
            BuildChatText();

            chatText.enabled = true;

            timer = StartCoroutine(DeactivateChat());
        }

        public void Show()
        {
            chatText.enabled = true;
        }

        public void Hide()
        {
            chatText.enabled = false;
        }

        protected void Awake()
        {
            SetupChatMessagesComponent();
            entries = new List<ChatLogEntry>();
        }

        private void SetupChatMessagesComponent()
        {
            chatEntry = new GameObject();
            chatText = chatEntry.AddComponent<GUIText>();
            chatEntry.AddComponent<GUITextShadow>();
            chatText.name = "ChatText";
            chatText.alignment = TextAlignment.Left;
            chatText.fontSize = 18;
            chatText.transform.position = new Vector3(0.05f, .5f, 1f);
            chatText.enabled = false;
            chatText.richText = true;
        }

        private void AddChatMessage(ChatLogEntry chatLogEntry)
        {
            if (entries.Count == MESSAGE_LIMIT)
            {
                entries.RemoveAt(0);
            }

            entries.Add(chatLogEntry);
        }

        private void BuildChatText()
        {
            string[] formattedEntries = entries
                .Select(entry => $"<color={entry.PlayerColor.AsHexString()}><b>{entry.PlayerName}: </b></color>{entry.MessageText}")
                .ToArray();

            chatText.text = string.Join("\n", formattedEntries);
        }

        private string SanitizeMessage(string message)
        {
            message = message.Trim();

            if (message.Length < LINE_CHAR_LIMIT)
            {
                return message;
            }

            return message.Substring(0, LINE_CHAR_LIMIT);
        }

        private IEnumerator DeactivateChat()
        {
            yield return new WaitForSeconds(CHAT_VISIBILITY_TIME_LENGTH);
            chatText.enabled = false;
            timer = null;
        }
    }
}
