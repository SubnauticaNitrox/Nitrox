using NitroxModel.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public static class LoadingScreenVersionText
{
    private static GameObject buildWatermark => uGUI.main.overlays.transform.parent.GetComponentInChildren<uGUI_BuildWatermark>().gameObject;

    private static uGUI_TextFade loadingScreenWarning;
    private static uGUI_TextFade versionText;

    public static void Initialize()
    {
        versionText = AddTextToLoadingScreen("LoadingScreenVersionText", $"\nNitrox {NitroxEnvironment.ReleasePhase} V{NitroxEnvironment.Version}");
        loadingScreenWarning = AddTextToLoadingScreen("LoadingScreenWarnText", $"\n\n{Language.main.Get("Nitrox_LoadingScreenWarn")}");
    }

    private static uGUI_TextFade AddTextToLoadingScreen(string name, string text)
    {
        GameObject gameObject = Object.Instantiate(buildWatermark, buildWatermark.transform.parent);
        gameObject.name = name;
        Object.Destroy(gameObject.GetComponent<uGUI_BuildWatermark>());

        uGUI_TextFade textFade = gameObject.AddComponent<uGUI_TextFade>();
        textFade.SetAlignment(TextAlignmentOptions.TopRight);
        textFade.SetColor(Color.white.WithAlpha(0.5f));
        textFade.SetText(text);
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
