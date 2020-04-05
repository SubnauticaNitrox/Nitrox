using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.Chat;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.GameLogic.ChatUI
{
    public class PlayerChatManager
    {
        private static PlayerChatManager main;
        public static PlayerChatManager Main
        {
            get
            {
                if (main == null)
                {
                    main = new PlayerChatManager
                    {
                        session = NitroxServiceLocator.LocateService<IMultiplayerSession>()
                    };
                    Player.main.StartCoroutine(main.LoadChatLogAsset());
                }

                return main;
            }
        }

        public PlayerChat PlayerChat;
        private IMultiplayerSession session;

        private bool finishedLoading;
        private bool showChatAfterLoading;
        private bool selectChatAfterLoading;
        private List<ChatLogEntry> entriesBeforeFinishedLoading = new List<ChatLogEntry>();

        public void ShowChat()
        {
            if (finishedLoading)
            {
                PlayerChat.SelectChat();
                PlayerChat.StartCoroutine(PlayerChat.ToggleTransparencyCanvasGroup(1f));
            }
            else
            {
                showChatAfterLoading = true;
            }
        }

        public void HideChat()
        {
            if (finishedLoading)
            {
                PlayerChat.DeselectChat();
                PlayerChat.StartCoroutine(PlayerChat.ToggleTransparencyCanvasGroup(0f));
            }
        }

        public void SelectChat()
        {
            if (finishedLoading)
            {
                ShowChat();
                PlayerChat.SelectChat();
            }
            else
            {
                selectChatAfterLoading = true;
            }
        }

        public void AddMessage(string playerName, string message, Color color)
        {
            ChatLogEntry entry = new ChatLogEntry(playerName, message, color);

            if (finishedLoading)
            {
                PlayerChat.WriteLogEntry(entry);
            }
            else
            {
                entriesBeforeFinishedLoading.Add(entry);
            }
        }

        public void SendMessage(string message)
        {
            session.Send(new ChatMessage(session.Reservation.PlayerId, message));
            PlayerChat.WriteLogEntry(new ChatLogEntry(session.AuthenticationContext.Username, message, session.PlayerSettings.PlayerColor));
        }

        private IEnumerator LoadChatLogAsset()
        {
            AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../AssetBundles/chatlog"));
            if (assetRequest == null)
            {
                Log.Error("Failed to load AssetBundle!");
                yield break;
            }

            while (!assetRequest.isDone)
            {
                yield return null;
            }

            string sceneName = assetRequest.assetBundle.GetAllScenePaths().First();
            Log.Debug($"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            GameObject oldPlayerChatCanvas = GameObject.Find("PlayerChatCanvas");
            Transform playerChatRoot = oldPlayerChatCanvas.transform.GetChild(0);
            playerChatRoot.SetParent(uGUI.main.screenCanvas.transform, false);
            Object.Destroy(oldPlayerChatCanvas);

            PlayerChat = playerChatRoot.gameObject.AddComponent<PlayerChat>();
            yield return PlayerChat.SetupChatComponents();

            foreach (ChatLogEntry entry in entriesBeforeFinishedLoading)
            {
                PlayerChat.WriteLogEntry(entry);
            }
            entriesBeforeFinishedLoading.Clear();
            finishedLoading = true;
            yield return new WaitForEndOfFrame();

            if (selectChatAfterLoading)
            {
                SelectChat();
            }
            else if (showChatAfterLoading)
            {
                ShowChat();
            }
        }


        public static void LoadChatKeyHint() => Player.main.StartCoroutine(LoadChatKeyHintAsset());

        private static IEnumerator LoadChatKeyHintAsset()
        {
            AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../AssetBundles/chatkeyhint"));
            if (assetRequest == null)
            {
                Log.Error("Failed to load AssetBundle!");
                yield break;
            }

            while (!assetRequest.isDone)
            {
                yield return null;
            }

            string sceneName = assetRequest.assetBundle.GetAllScenePaths().First();
            Log.Debug($"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            GameObject oldPlayerChatCanvas = GameObject.Find("ChatKeyCanvas");
            Transform playerChatRoot = oldPlayerChatCanvas.transform.GetChild(0);
            playerChatRoot.SetParent(uGUI.main.screenCanvas.transform, false);
            Object.Destroy(oldPlayerChatCanvas);
        }
    }
}
