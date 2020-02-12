using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


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

        private Text chatText;
        private List<ChatLogEntry> entries;
        private Coroutine timer;
        private Coroutine chatScene;
        private GameObject chatScrollRect;
        private GameObject mainChatLog;
        AssetBundleCreateRequest asset;
 
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
            timer = StartCoroutine(DeactivateChat());

        }

        public void Show()
        {
            if(mainChatLog != null)
            {
                mainChatLog.SetActive(true);
                EventSystem.current.SetSelectedGameObject(chatScrollRect);
            }  
        }

        public void Hide()
        {
            if (mainChatLog != null)
            {
                mainChatLog.SetActive(false);
            }
        }

        protected void Awake()
        {
            chatScene = StartCoroutine(LoadChatLogAsset());
            entries = new List<ChatLogEntry>();
        }

        private void SetupChatMessagesComponent()
        {
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

        private IEnumerator DeactivateChat()
        {
            yield return new WaitForSeconds(CHAT_VISIBILITY_TIME_LENGTH);
            if (mainChatLog != null)
            {
                mainChatLog.SetActive(false);
            }
            timer = null;
        }

        private IEnumerator LoadChatLogAsset()
        {
            asset = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../AssetBundles/chatlog"));
            if (asset == null)
            {
                Log.Instance.LogMessage(LogCategory.Info, "Failed to load AssetBundle!");
                yield break;
            }

            while (!asset.isDone)
            {
                yield return null;
            }

            string sceneName = asset.assetBundle.GetAllScenePaths().First();
            Log.Instance.LogMessage(LogCategory.Info, $"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            chatScrollRect = GameObject.Find("ChatLogContent");
            mainChatLog = GameObject.Find("ChatLogScrollView");
            SetupChatMessagesComponent();
        }
    }
}
