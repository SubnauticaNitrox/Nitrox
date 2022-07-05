using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class LoadingScreenVersionText
    {
        private static GameObject loadingTextGameObject => uGUI.main.loading.loadingText.gameObject;

        private static uGUI_TextFade loadingScreenWarning;
        private static uGUI_TextFade versionText;

        public static void Initialize()
        {
            versionText = AddTextToLoadingScreen("LoadingScreenVersionText", $"\nNitrox {NitroxEnvironment.ReleasePhase} V{NitroxEnvironment.Version}");
            loadingScreenWarning = AddTextToLoadingScreen("LoadingScreenWarnText", $"\n\n{Language.main.Get("Nitrox_LoadingScreenWarn")}");
        }

        private static uGUI_TextFade AddTextToLoadingScreen(string name, string text)
        {
            GameObject gameObject = Object.Instantiate(loadingTextGameObject, loadingTextGameObject.transform.parent);
            gameObject.name = name;

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
