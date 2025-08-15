using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class NitroxBootstrapper : MonoBehaviour
{
    internal static NitroxBootstrapper Instance;

    // Awake is too early in Subnautica's lifecycle to access PlatformUtils
    // so we pick Start which will always happen after it's initialized
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        gameObject.AddComponent<SceneCleanerPreserve>();
        gameObject.AddComponent<NitroxMainMenuModifications>();
        gameObject.AddComponent<DiscordClient>();

#if DEBUG
        EnableDeveloperFeatures();
        CreateDebugger();
#endif

        // This is very important, see Application_runInBackground_Patch.cs
        Application.runInBackground = true;
        Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        // Also very important for similar reasons
        MiscSettings.pdaPause = false;
    }

#if DEBUG
    private static void EnableDeveloperFeatures()
    {
        Log.Info("Enabling Subnautica developer console");
        PlatformUtils.SetDevToolsEnabled(true);
    }

    private void CreateDebugger()
    {
        Log.Info("Enabling Nitrox debugger");
        GameObject debugger = new();
        debugger.name = "Debug manager";
        debugger.AddComponent<NitroxDebugManager>();
        debugger.transform.SetParent(transform);
    }
#endif
}
