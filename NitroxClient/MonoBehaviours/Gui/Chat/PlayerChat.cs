using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private List<ChatMessage> messages;

        protected void Awake()
        {
            SetupChatMessagesComponent();

            messages = new List<ChatMessage>();
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

        public void WriteMessage(string message, Color color)
        {
            if (timer != null)
            {
                // cancel hiding chat messages because a new one was recently posted
                StopCoroutine(timer);
            }
            AddChatMessage(SanitizeMessage(message), color);
            BuildChatText();

            chatText.enabled = true;

            timer = StartCoroutine(DeactivateChat());
        }

        private void AddChatMessage(string sanitizedChatMessage, Color color)
        {
            if (messages.Count == MESSAGE_LIMIT)
            {
                messages.RemoveAt(0);
            }

            messages.Add(new ChatMessage(sanitizedChatMessage, color));
        }

        private void BuildChatText()
        {
            chatText.text = string.Join("\n", messages.Select(m => "<color=#" + ColorUtility.ToHtmlStringRGB(m.Color) + ">" + m.Text + "</color>").ToArray());
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

        public class ChatMessage
        {
            public readonly string Text;
            public Color Color;

            public ChatMessage(string text, Color color)
            {
                Text = text;
                Color = color;
            }
        }
    }
}
