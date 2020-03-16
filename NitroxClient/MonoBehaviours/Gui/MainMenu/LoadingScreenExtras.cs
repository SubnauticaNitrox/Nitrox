using System.Reflection;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class LoadingScreenExtras
    {
        private static string assemblyVersion => Assembly.GetAssembly(typeof(LoadingScreenExtras)).GetName().Version.ToString();
        private static GameObject loadingTextGameObject => uGUI.main.loading.loadingText.gameObject;

        private static GameObject nitroxAlphaGameObject;
        private static uGUI_TextFade nitroxAlphaText;

        public static void Initialize()
        {
            if (nitroxAlphaGameObject == null)
            {
                nitroxAlphaGameObject = Object.Instantiate(loadingTextGameObject, loadingTextGameObject.transform.parent);
                nitroxAlphaGameObject.name = "LoadingScreenNitroxAlpha";
            }

            if (nitroxAlphaText == null)
            {
                nitroxAlphaText = nitroxAlphaGameObject.GetComponent<uGUI_TextFade>();
                nitroxAlphaText.SetText("Nitrox Alpha V" + assemblyVersion + "\nExpect game breaking bugs.");
                nitroxAlphaText.SetAlignment(TextAnchor.UpperRight);
            }

            nitroxAlphaText.FadeIn(1f, null);
        }

        public static void Disable()
        {
            nitroxAlphaText.FadeOut(1f, null);
        }
    }
}
