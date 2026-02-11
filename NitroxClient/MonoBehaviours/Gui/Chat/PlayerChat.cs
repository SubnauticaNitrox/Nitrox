using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nitrox.Model.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChat : uGUI_InputGroup
    {
        private const int LINE_CHAR_LIMIT = 255;
        private const int MESSAGES_LIMIT = 64;
        private const float TOGGLED_TRANSPARENCY = 0.4f;

        private static readonly Queue<ChatLogEntry> entries = [];
        private Image[] backgroundImages;
        private CanvasGroup canvasGroup;
        private Coroutine? fadeCoroutine;
        private InputField inputField;
        private GameObject logEntryPrefab;

        private bool transparent;

        public static bool IsReady { get; private set; }

        public string InputText
        {
            get => inputField.text;
            set
            {
                inputField.text = value;
                inputField.caretPosition = value.Length;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!focused)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(InputText))
            {
                return;
            }

            // Handle command auto complete.
            if (InputText[0] == '/' && UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                // Auto complete command names.
                if (Regex.IsMatch(InputText, @"^/\w+$"))
                {
                    string commandName = InputText.Substring(1);
                    this.Resolve<IPacketSender>().Send(new TextAutoComplete(commandName, TextAutoComplete.AutoCompleteContext.COMMAND_NAME));
                }
            }
        }

        public IEnumerator SetupChatComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            logEntryPrefab = GameObject.Find("ChatLogEntryPrefab");
            logEntryPrefab.AddComponent<PlayerChatLogItem>();
            logEntryPrefab.SetActive(false);

            GetComponentsInChildren<Button>()[0].onClick.AddListener(ToggleBackgroundTransparency);
            GetComponentsInChildren<Button>()[1].gameObject.AddComponent<PlayerChatPinButton>();

            inputField = GetComponentInChildren<InputField>();
            inputField.gameObject.AddComponent<PlayerChatInputField>().InputField = inputField;
            inputField.GetComponentInChildren<Button>().onClick.AddListener(PlayerChatManager.Instance.SendMessage);

            // We pick any image that's inside the chat component to have all of their opacity lowered
            backgroundImages = transform.GetComponentsInChildren<Image>();

            yield return Yielders.WaitForEndOfFrame; //Needed so Select() works on initialization
            IsReady = true;
        }

        public IEnumerator WriteLogEntry(string playerName, string message, Color color)
        {
            // TODO: in the future, reuse the last message as the new message to avoid the cost of Instantiate
            if (entries.Count == MESSAGES_LIMIT)
            {
                Destroy(entries.Dequeue().EntryObject);
                yield return null; // Skips one frame to ensure the object is destroyed
            }

            ChatLogEntry chatLogEntry;
            GameObject chatLogEntryObject;
            if (entries.Count != 0 && entries.Last().PlayerName == playerName)
            {
                chatLogEntry = entries.Last();
                chatLogEntry.MessageText += $"{Environment.NewLine}{message}";
                chatLogEntry.UpdateTime();
                chatLogEntryObject = chatLogEntry.EntryObject;
            }
            else
            {
                chatLogEntry = new ChatLogEntry(playerName, SanitizeMessage(message), color);
                chatLogEntryObject = Instantiate(logEntryPrefab, logEntryPrefab.transform.parent, false);
                chatLogEntry.EntryObject = chatLogEntryObject;
                entries.Enqueue(chatLogEntry);
            }

            chatLogEntryObject.GetComponent<PlayerChatLogItem>().ApplyOnPrefab(chatLogEntry);
        }

        public void Show()
        {
            PlayerChatInputField.ResetTimer();
            // Stop any existing fade to prevent race condition
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(ToggleChatFade(true));
        }

        public void Hide()
        {
            // Stop any existing fade to prevent race condition
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(ToggleChatFade(false));
        }

        public void Select()
        {
            base.Select(true);
            inputField.Select();
            inputField.ActivateInputField();
        }

        private static string SanitizeMessage(string message)
        {
            message = message.Trim().TrimEnd('\n').Trim();
            return message.Length < LINE_CHAR_LIMIT ? message : message.Substring(0, LINE_CHAR_LIMIT);
        }

        private void ToggleBackgroundTransparency()
        {
            float alpha = transparent ? 1f : TOGGLED_TRANSPARENCY;
            transparent = !transparent;

            foreach (Image backgroundImage in backgroundImages)
            {
                backgroundImage.CrossFadeAlpha(alpha, 0.5f, false);
            }
        }

        private IEnumerator ToggleChatFade(bool fadeIn)
        {
            if (fadeIn)
            {
                while (canvasGroup.alpha < 1f)
                {
                    canvasGroup.alpha += 0.01f;
                    yield return null;
                }
            }
            else
            {
                while (canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha -= 0.01f;
                    yield return null;
                }
            }
        }
    }
}
