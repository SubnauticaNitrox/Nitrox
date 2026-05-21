using System;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class NitroxBootstrapper : MonoBehaviour
{
    private const string DISABLE_DISCORD_INTEGRATION_ARG = "--disable-discord-integration";

    internal static NitroxBootstrapper Instance;

    public static bool IsDiscordIntegrationDisabled =>
        Array.Exists(Environment.GetCommandLineArgs(), arg => arg.Equals(DISABLE_DISCORD_INTEGRATION_ARG, StringComparison.OrdinalIgnoreCase));

    // Awake is too early in Subnautica's lifecycle to access PlatformUtils
    // so we pick Start which will always happen after it's initialized
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        gameObject.AddComponent<SceneCleanerPreserve>();
        gameObject.AddComponent<NitroxMainMenuModifications>();

        if (IsDiscordIntegrationDisabled)
        {
            Log.Info("[Discord] Discord integration disabled by launch argument");
        }
        else
        {
            gameObject.AddComponent<DiscordClient>();
        }

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
