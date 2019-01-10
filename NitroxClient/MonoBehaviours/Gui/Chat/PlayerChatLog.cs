using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    /// <summary>
    ///     The Unity object that holds a record of what was said in-game.
    /// </summary>
    internal class PlayerChatLog : MonoBehaviour
    {
        private const int LINE_CHAR_LIMIT = 80;
        private const int MESSAGE_LIMIT = 255;
        private const float CHAT_VISIBILITY_TIME_LENGTH = 7f;

        private Text chatText;
        private List<ChatLogEntry> entries;
        private Coroutine timer;
        private Coroutine chatScene;
        private GameObject SomeScrollRect;
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
            if(SomeScrollRect != null)
            {
                mainChatLog.SetActive(true);
            }  
        }

        public void Hide()
        {
            if (SomeScrollRect != null)
            {
                mainChatLog.SetActive(true);
            }
        }

        protected void Awake()
        {
            chatScene = StartCoroutine(LoadChatLogAsset());
            entries = new List<ChatLogEntry>();
        }

        private void SetupChatMessagesComponent()
        {
            chatText = SomeScrollRect.AddComponent<Text>();
            chatText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            chatText.name = "ChatText";
            chatText.fontSize = 12;
            chatText.alignment = TextAnchor.MiddleLeft;
            Log.Info("SCROLLRECT: " + SomeScrollRect + "ChatText: " + chatText);
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
            Log.Info(" NEW ChatText: " + chatText.text);
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
            if (SomeScrollRect != null)
            {
                SomeScrollRect.SetActive(true);
            }
            timer = null;
        }

        private IEnumerator LoadChatLogAsset()
        {
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
            SomeScrollRect = GameObject.Find("ChatLogContent");
            mainChatLog = GameObject.Find("ChatLogScrollView");
            SetupChatMessagesComponent();
        }
    }
}
