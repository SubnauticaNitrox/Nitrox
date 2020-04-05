using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChat : uGUI_InputGroup
    {
        private const int LINE_CHAR_LIMIT = 255;
        private const int MESSAGE_LIMIT = 64;
        private const float TOGGLED_TRANSPARENCY = 0.4f;
        public const float CHAT_VISIBILITY_TIME_LENGTH = 10f;

        private CanvasGroup canvasGroup;
        private HorizontalOrVerticalLayoutGroup[] layoutGroups;
        private GameObject logEntryPrefab;
        private Image[] backgroundImages;
        private bool transparent;
        private InputField inputField;

        private Dictionary<ChatLogEntry, GameObject> entries = new Dictionary<ChatLogEntry, GameObject>();

        public IEnumerator SetupChatComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            layoutGroups = GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();

            logEntryPrefab = GameObject.Find("ChatLogEntryPrefab");
            logEntryPrefab.AddComponent<PlayerChatLogItem>();
            logEntryPrefab.SetActive(false);

            GetComponentsInChildren<Button>()[0].onClick.AddListener(ToggleTransparency);
            GetComponentsInChildren<Button>()[1].gameObject.AddComponent<PlayerChatPinButton>();

            inputField = GetComponentInChildren<InputField>();
            inputField.gameObject.AddComponent<PlayerChatInputField>();
            inputField.GetComponentInChildren<Button>().onClick.AddListener(SendChatMessage);

            backgroundImages = new[]
            {
                transform.GetChild(0).GetComponent<Image>(), transform.GetChild(1).GetComponent<Image>(), transform.GetChild(3).GetComponent<Image>()
            };
            yield return new WaitForEndOfFrame();
        }

        public void WriteLogEntry(ChatLogEntry chatLogEntry)
        {
            chatLogEntry.MessageText = SanitizeMessage(chatLogEntry.MessageText);

            if (entries.Count == MESSAGE_LIMIT)
            {
                Destroy(entries.Values.First());
                entries.Remove(entries.Keys.First());
            }

            GameObject newChatLogEntry = Instantiate(logEntryPrefab, logEntryPrefab.transform.parent);
            newChatLogEntry.GetComponent<PlayerChatLogItem>().ApplyOnPrefab(chatLogEntry);
            entries.Add(chatLogEntry, newChatLogEntry);

            StartCoroutine(UpdateChatEntrySpacing());
        }

        // ReSharper disable once Unity.InefficientPropertyAccess
        // Updates the layout sorting algorithm from Unity to prevent "loss" of text messages.
        private IEnumerator UpdateChatEntrySpacing()
        {
            yield return new WaitForEndOfFrame();
            foreach (HorizontalOrVerticalLayoutGroup layoutGroup in layoutGroups)
            {
                layoutGroup.enabled = false;
            }
            yield return new WaitForEndOfFrame();
            foreach (HorizontalOrVerticalLayoutGroup layoutGroup in layoutGroups)
            {
                layoutGroup.enabled = true;
            }
        }

        public void SendChatMessage()
        {
            if (inputField.text.Trim() != "")
            {
                PlayerChatManager.Main.SendMessage(inputField.text);
                inputField.text = "";
            }
        }

        public void SelectChat()
        {
            Select(true);
            inputField.Select();
            inputField.ActivateInputField();
        }

        public void DeselectChat()
        {
            Deselect();
            EventSystem.current.SetSelectedGameObject(null);
        }

        private static string SanitizeMessage(string message)
        {
            message = message.Trim();
            return message.Length < LINE_CHAR_LIMIT ? message : message.Substring(0, LINE_CHAR_LIMIT);
        }

        private void ToggleTransparency()
        {
            float alpha = transparent ? 1f : TOGGLED_TRANSPARENCY;
            transparent = !transparent;

            foreach (Image backgroundImage in backgroundImages)
            {
                backgroundImage.CrossFadeAlpha(alpha, 0.5f, false);
            }
        }

        public IEnumerator ToggleTransparencyCanvasGroup(float finalAlpha)
        {
            float step = finalAlpha == 1f ? 0.01f : -0.01f;

            while (canvasGroup.alpha != finalAlpha)
            {
                canvasGroup.alpha += step;
                yield return new WaitForSeconds(0.004f);
            }
        }
    }
}
