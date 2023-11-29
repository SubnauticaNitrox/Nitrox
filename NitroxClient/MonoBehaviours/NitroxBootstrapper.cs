using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class NitroxBootstrapper : MonoBehaviour
{
    internal static NitroxBootstrapper Instance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        gameObject.AddComponent<SceneCleanerPreserve>();
        gameObject.AddComponent<MainMenuMods>();
        gameObject.AddComponent<DiscordClient>();

#if DEBUG
        EnableDeveloperFeatures();
#endif

        CreateDebugger();

        // This is very important, see Application_runInBackground_Patch.cs
        Application.runInBackground = true;
        Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
    }

    private void EnableDeveloperFeatures()
    {
        Log.Info("Enabling developer console.");
        PlatformUtils.SetDevToolsEnabled(true);
    }

    private void CreateDebugger()
    {
        GameObject debugger = new GameObject();
        debugger.name = "Debug manager";
        debugger.AddComponent<NitroxDebugManager>();
        debugger.transform.SetParent(transform);
    }
}
