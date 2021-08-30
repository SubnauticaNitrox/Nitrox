﻿using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class LoadingScreenVersionText
    {
        private static string assemblyVersion => Assembly.GetAssembly(typeof(LoadingScreenVersionText)).GetName().Version.ToString();
        private static GameObject loadingTextGameObject => uGUI.main.loading.loadingText.gameObject;

        private static uGUI_TextFade loadingScreenWarning;
        private static uGUI_TextFade versionText;

        public static void Initialize()
        {
            versionText = AddTextToLoadingScreen($"\nNitrox Alpha V{assemblyVersion}");
            loadingScreenWarning = AddTextToLoadingScreen($"\n\n{Language.main.Get("Nitrox_LoadingScreenWarn")}");
        }

        private static uGUI_TextFade AddTextToLoadingScreen(string text)
        {
            GameObject gameObject = Object.Instantiate(loadingTextGameObject, loadingTextGameObject.transform.parent);
            gameObject.name = "LoadingScreenVersionText";

            uGUI_TextFade textFade = gameObject.GetComponent<uGUI_TextFade>();
            textFade.SetText(text);
            textFade.SetAlignment(TextAnchor.UpperRight);
            textFade.FadeIn(1f, null);

            return textFade;
        }

        public static void DisableWarningText()
        {
            loadingScreenWarning.FadeOut(1f, null);
            if (XRSettings.enabled)
            {
                versionText.FadeOut(1f, null);
            }
        }
    }
}
