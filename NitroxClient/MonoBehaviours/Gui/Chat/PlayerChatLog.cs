using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    /// <summary>
    ///     The Unity object that holds a record of what was said in-game.
    /// </summary>
    internal class PlayerChatLog : MonoBehaviour
    {
        private const int LINE_CHAR_LIMIT = 255;
        private const int MESSAGE_LIMIT = 255;
        private const float CHAT_VISIBILITY_TIME_LENGTH = 7f;
        private AssetBundleCreateRequest asset;
        private bool assetsLoaded;
        private GameObject chatScrollRect;
        private Text chatText;
        private List<ChatLogEntry> entries;
        private GameObject mainChatLog;
        private float timeLeftUntilAutoClose;

        public PlayerChat Manager { get; set; }
        private bool isChatAvailable => Manager != null && assetsLoaded && mainChatLog && chatText;

        /// <summary>
        ///     Takes a new chat message and displays that message along with MESSAGE_LIMIT-1 previous entries for
        ///     CHAT_VISIBILITY_TIME_LENGTH seconds
        /// </summary>
        /// <param name="chatLogEntry"></param>
        public void WriteEntry(ChatLogEntry chatLogEntry)
        {
            // Always store chat messages received from other players even if this player can't chat yet.
            chatLogEntry.MessageText = SanitizeMessage(chatLogEntry.MessageText);
            AddChatMessage(chatLogEntry);
            
            if (isChatAvailable)
            {
                BuildChatText();
            }
        }

        public void Show()
        {
            timeLeftUntilAutoClose = CHAT_VISIBILITY_TIME_LENGTH;
            if (isChatAvailable)
            {
                mainChatLog.SetActive(true);
                EventSystem.current.SetSelectedGameObject(chatScrollRect);
            }
        }

        public void Hide()
        {
            if (isChatAvailable)
            {
                mainChatLog.SetActive(false);
            }
        }

        protected void Awake()
        {
            entries = new List<ChatLogEntry>();
            StartCoroutine(LoadChatLogAsset());
        }

        protected void LateUpdate()
        {
            if (!isChatAvailable || Manager.inputField.ChatEnabled)
            {
                timeLeftUntilAutoClose = CHAT_VISIBILITY_TIME_LENGTH;
                return;
            }

            timeLeftUntilAutoClose -= Time.unscaledDeltaTime;
            if (timeLeftUntilAutoClose <= 0)
            {
                Hide();
            }
        }

        private void SetupChatMessagesComponent()
        {
            if (isChatAvailable)
            {
                return;
            }

            chatText = new GameObject("ChatText").AddComponent<Text>();
            chatText.transform.SetParent(chatScrollRect.transform);
            chatText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            chatText.name = "ChatText";
            chatText.alignByGeometry = false;
            chatText.horizontalOverflow = HorizontalWrapMode.Wrap;
            chatText.verticalOverflow = VerticalWrapMode.Truncate;
            chatText.fontSize = 17;
            chatText.fontStyle = FontStyle.Bold;
            chatText.lineSpacing = 0.8f;
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

        private IEnumerator LoadChatLogAsset()
        {
            if (isChatAvailable)
            {
                yield break;
            }

            asset = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../AssetBundles/chatlog"));
            if (asset == null)
            {
                Log.Info("Failed to load AssetBundle!");
                yield break;
            }

            while (!asset.isDone)
            {
                yield return null;
            }

            string sceneName = asset.assetBundle.GetAllScenePaths().First();
            Log.Info($"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            chatScrollRect = GameObject.Find("ChatLogContent");
            mainChatLog = GameObject.Find("ChatLogScrollView");
            SetupChatMessagesComponent();
            assetsLoaded = true;

            // Init build log text if other players have send messages before and this player joins after
            if (entries.Count > 0)
            {
                BuildChatText();
            }
        }
    }
}
