using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    class PlayerChat : MonoBehaviour
    {
        private const int LINE_CHAR_LIMIT = 80;
        private const int MESSAGE_LIMIT = 6;
        private const float CHAT_VISIBILITY_TIME_LENGTH = 7f;

        private GameObject chatEntry;
        private GUIText chatText;
        private Coroutine timer;
        private List<string> messages;

        protected void Awake()
        {
            SetupChatMessagesComponent();

            messages = new List<string>();
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
        }

        // Takes a new chat message and displays that message along with MESSAGE_LIMIT-1 previous messages for CHAT_VISIBILITY_TIME_LENGTH seconds
        public void WriteMessage(string message)
        {
            if (timer != null)
            {
                // cancel hiding chat messages because a new one was recently posted
                StopCoroutine(timer);
            }

            AddChatMessage(SanitizeMessage(message));
            BuildChatText();

            chatText.enabled = true;

            timer = StartCoroutine(DeactivateChat());
        }

        private void AddChatMessage(string sanitizedChatMessage)
        {
            if (messages.Count == MESSAGE_LIMIT)
            {
                messages.RemoveAt(0);
            }

            messages.Add(sanitizedChatMessage);
        }

        private void BuildChatText()
        {
            chatText.text = "";
            foreach (string message in messages)
            {
                if (chatText.text.Length > 0)
                {
                    chatText.text += "\n";
                }

                chatText.text += message;
            }
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
