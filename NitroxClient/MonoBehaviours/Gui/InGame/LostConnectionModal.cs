using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace NitroxClient.MonoBehaviours.Gui.InGame
{
    // TODO: Prevent closing of window when clicking outside of it.
    // TODO: Refactor to a standalone modal so that it can be used in the main menu.
    /// <summary>
    ///     Extends the IngameMenu with a disconnect popup.
    /// </summary>
    public class LostConnectionModal : MonoBehaviour
    {
        public const string SUB_WINDOW_NAME = "LostConnection";
        private static GameObject lostConnectionSubWindow;
        public static LostConnectionModal Instance { get; private set; }

        private static GameObject loadingTextGameObject => uGUI.main.loading.loadingText.gameObject;
        private static uGUI_TextFade lostConnectionText;

        public void Show()
        {
            FreezeTime.Begin("NitroxDisconnected");
            StartCoroutine(Show_Impl());
            // Inspired by LoadingScreenVersionText.cs
            lostConnectionText = MakeLostConnectionText(Language.main.Get("Nitrox_LostConnectionWarn"));
        }

        private static void InitSubWindow()
        {
            if (!IngameMenu.main)
            {
                throw new NotSupportedException($"Cannot show ingame subwindow {SUB_WINDOW_NAME} because the ingame window does not exist.");
            }

            if (!lostConnectionSubWindow)
            {
                GameObject derivedSubWindow = IngameMenu.main.transform.Find("QuitConfirmation").gameObject;
                lostConnectionSubWindow = Instantiate(derivedSubWindow, IngameMenu.main.transform, false);
                lostConnectionSubWindow.name = SUB_WINDOW_NAME;

                // Styling.
                RectTransform main = lostConnectionSubWindow.GetComponent<RectTransform>();
                main.sizeDelta = new Vector2(700, 195);

                DestroyImmediate(lostConnectionSubWindow.FindChild("ButtonNo")); // Delete Button No

                GameObject header = lostConnectionSubWindow.FindChild("Header"); //Message Object

                header.GetComponent<Text>().text = Language.main.Get("Nitrox_LostConnection");

                RectTransform messageTransform = header.GetComponent<RectTransform>();
                messageTransform.sizeDelta = new Vector2(700, 195);

                GameObject buttonYes = lostConnectionSubWindow.FindChild("ButtonYes"); //Button Yes Object
                buttonYes.transform.position = new Vector3(lostConnectionSubWindow.transform.position.x / 2, buttonYes.transform.position.y, buttonYes.transform.position.z); // Center Button

                buttonYes.GetComponentInChildren<Text>().text = "OK"; //Set Button Text Component
            }
        }

        private void Start()
        {
            if (Instance)
            {
                throw new NotSupportedException($"Only one {nameof(LostConnectionModal)} must be active at any time.");
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private IEnumerator Show_Impl()
        {
            // Execute frame-by-frame to allow UI scripts to initialize.
            InitSubWindow();
            yield return null;
            IngameMenu.main.Open();
            yield return null;
            IngameMenu.main.ChangeSubscreen(SUB_WINDOW_NAME);
        }

        private static uGUI_TextFade MakeLostConnectionText(string text)
        {
            GameObject gameObject = Instantiate(loadingTextGameObject, loadingTextGameObject.transform.parent);
            gameObject.name = "LoadingScreenLostConnection";

            uGUI_TextFade textFade = gameObject.GetComponent<uGUI_TextFade>();
            textFade.SetColor(Color.red);
            // We jump 2 lines so that it's written after the versison text
            textFade.SetText($"\n\n{text}");
            textFade.SetAlignment(TextAnchor.UpperRight);
            textFade.FadeIn(1f, null);

            return textFade;
        }

        // TODO: Call this when returning to the main menu will be allowed
        public static void DisableConnectionLostText()
        {
            if (!lostConnectionText)
            {
                return;
            }

            lostConnectionText.FadeOut(1f, null);
        }
    }
}
