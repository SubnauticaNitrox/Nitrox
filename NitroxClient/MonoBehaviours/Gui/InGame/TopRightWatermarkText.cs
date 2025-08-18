using NitroxModel.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public static class TopRightWatermarkText
{
    private static GameObject buildWatermark => uGUI.main.overlays.transform.parent.GetComponentInChildren<uGUI_BuildWatermark>().gameObject;

    private static uGUI_TextFade loadingScreenWarning;
    private static uGUI_TextFade versionText;

    public static void Initialize()
    {
        versionText = AddTextToLoadingScreen("LoadingScreenVersionText", $"\nNitrox {NitroxEnvironment.VersionInfo}");
        loadingScreenWarning = AddTextToLoadingScreen("LoadingScreenWarnText", $"\n\n{Language.main.Get("Nitrox_LoadingScreenWarn")}");
    }

    private static uGUI_TextFade AddTextToLoadingScreen(string name, string text)
    {
        GameObject gameObject = Object.Instantiate(buildWatermark, buildWatermark.transform.parent);
        gameObject.name = name;
        Object.Destroy(gameObject.GetComponent<uGUI_BuildWatermark>());
        Object.Destroy(gameObject.GetComponent<TextMeshProUGUI>()); // This forces TextMeshPro to redo text decorations which it would otherwise cache.

        uGUI_TextFade textFade = gameObject.AddComponent<uGUI_TextFade>(); // This also adds a "TextMeshProUGUI" component.
        textFade.SetAlignment(TextAlignmentOptions.TopRight);
        textFade.SetColor(Color.white.WithAlpha(0.4f));
        textFade.SetText(text);
        textFade.FadeIn(1f, null);

        return textFade;
    }

    public static void ApplyChangesForInGame()
    {
        versionText.text.fontSize = 18;
        versionText.GetComponent<CanvasGroup>().alpha = 0.2f; // TextMeshPro alpha stops working when in-game so we use canvas alpha.
        if (XRSettings.enabled)
        {
            versionText.FadeOut(1f, null);
        }
        loadingScreenWarning.FadeOut(1f, () => Object.Destroy(loadingScreenWarning.gameObject));
    }
}
