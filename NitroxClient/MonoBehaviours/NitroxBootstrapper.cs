using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NitroxClient.MonoBehaviours.Core;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxClient.Services.Game;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

internal sealed class NitroxBootstrapper : MonoBehaviour
{
    public static NitroxBootstrapper Instance = null!;

    // Awake is too early in Subnautica's lifecycle to access PlatformUtils
    // so we pick Start which will always happen after it's initialized
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<SceneCleanerPreserve>();
        gameObject.AddComponent<MainMenuModsService>();
        gameObject.AddComponent<DiscordClientService>();

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

    /// <summary>
    ///     Sets up hosted services that can run code on the main game thread.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        GameObject nitroxRoot = new();
        nitroxRoot.name = "Nitrox";
        nitroxRoot.AddComponent<NitroxBootstrapper>();
        NitroxServicesManager servicesManager = nitroxRoot.AddComponent<NitroxServicesManager>();
        servicesManager.GameServices = serviceProvider.GetRequiredService<IEnumerable<IGameService>>().ToArray();
        servicesManager.MultiplayerServices = serviceProvider.GetRequiredService<IEnumerable<IMultiplayerGameService>>().ToArray();
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
