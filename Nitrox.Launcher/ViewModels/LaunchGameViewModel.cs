using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Model.Platforms.Store;

namespace Nitrox.Launcher.ViewModels;

internal partial class LaunchGameViewModel(DialogService dialogService, ServerService serverService, OptionsViewModel optionsViewModel, IKeyValueStore keyValueStore)
    : RoutableViewModelBase
{
    public static Task<string>? LastFindSubnauticaTask;
    private const string DISABLE_DISCORD_INTEGRATION_ARG = "--disable-discord-integration";
    private static readonly TimeSpan DiscordCrashObservationPeriod = TimeSpan.FromSeconds(45);
    private static readonly string[] DiscordCrashSignatures =
    [
        "discord_game_sdk",
        "DiscordClient:UpdateActivity",
        "DiscordClient:InitializeRPMenu",
        "DiscordGameSDKWrapper.ActivityManager:UpdateActivity"
    ];
    private static bool hasInstantLaunched;
    private readonly DialogService dialogService = dialogService;
    private readonly IKeyValueStore keyValueStore = keyValueStore;

    private readonly ServerService serverService = serverService;

    [ObservableProperty]
    public partial Platform GamePlatform { get; set; }

    [ObservableProperty]
    public partial string? PlatformToolTip { get; set; }

    public Bitmap[] GalleryImageSources { get; } =
    [
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-1.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-2.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-3.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-4.png", static stream => new Bitmap(stream))
    ];

    public string Version => $"{NitroxEnvironment.ReleasePhase} {NitroxEnvironment.Version}";

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            GamePlatform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
            PlatformToolTip = GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "";
            HandleInstantLaunchForDevelopment();
        }, cancellationToken);
    }

    internal override Task ViewContentUnloadAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartSingleplayerAsync()
    {
        if (GameInspect.WarnIfGameProcessExists(GameInfo.Subnautica) && !keyValueStore.GetIsMultipleGameInstancesAllowed())
        {
            return;
        }

        Log.Info("Launching Subnautica in singleplayer mode");
        try
        {
            if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
            {
                ChangeView(optionsViewModel);
                LauncherNotifier.Warning("Location of Subnautica is unknown. Set the path to it in settings");
                return;
            }

            NitroxEntryPatch.Remove(NitroxUser.GamePath);
            await StartSubnauticaAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while starting game in singleplayer mode:");
            await dialogService.ShowErrorAsync(ex, "Error while starting game in singleplayer mode");
        }
    }

    /// <summary>
    ///     Prepares Subnautica to load Nitrox, then starts Subnautica.
    /// </summary>
    [RelayCommand]
    private async Task StartMultiplayerAsync(string[]? args = null)
    {
        Log.Info("Launching Subnautica in multiplayer mode");
        try
        {
            bool setupResult = await Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
                {
                    ChangeView(optionsViewModel);
                    LauncherNotifier.Warning("Location of Subnautica is unknown. Set the path to it in settings");
                    return false;
                }
                if (PirateDetection.HasTriggered)
                {
                    LauncherNotifier.Error("Aarrr! Nitrox has walked the plank :(");
                    return false;
                }
                if (GameInspect.WarnIfGameProcessExists(GameInfo.Subnautica) && !keyValueStore.GetIsMultipleGameInstancesAllowed())
                {
                    return false;
                }
                if (await GameInspect.IsOutdatedGameAndNotify(NitroxUser.GamePath, dialogService))
                {
                    return false;
                }

                // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory
                try
                {
                    const string PATCHER_DLL_NAME = "NitroxPatcher.dll";

                    string patcherDllPath = Path.Combine(NitroxUser.ExecutableRootPath, "lib", "net472", PATCHER_DLL_NAME);
                    if (!File.Exists(patcherDllPath))
                    {
                        LauncherNotifier.Error("Launcher files seems corrupted, please contact us");
                        return false;
                    }

                    File.Copy(
                        patcherDllPath,
                        Path.Combine(NitroxUser.GamePath, GameInfo.Subnautica.DataFolder, "Managed", PATCHER_DLL_NAME),
                        true
                    );
                }
                catch (IOException ex)
                {
                    Log.Error(ex, "Unable to move initialization dll to Managed folder. Still attempting to launch because it might exist from previous runs");
                }

                // Try inject Nitrox into Subnautica code.
                if (LastFindSubnauticaTask != null)
                {
                    await LastFindSubnauticaTask;
                }
                await NitroxEntryPatch.Apply(NitroxUser.GamePath);

                if (QModHelper.IsQModInstalled(NitroxUser.GamePath))
                {
                    Log.Warn("Seems like QModManager is installed");
                    LauncherNotifier.Warning("QModManager Detected in the game folder");
                }
                GameInspect.WarnIfBepInExMods(NitroxUser.GamePath);

                return true;
            });

            if (!setupResult)
            {
                return;
            }

            await StartSubnauticaAsync(args);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while starting game in multiplayer mode:");
            await dialogService.ShowErrorAsync(ex, "Error while starting game in multiplayer mode");
        }
    }

    [RelayCommand]
    private void OpenContributionsOfYear()
    {
        string fromValue = HttpUtility.UrlEncode($"{DateTime.UtcNow.AddYears(-1):M/d/yyyy}");
        string toValue = HttpUtility.UrlEncode($"{DateTime.UtcNow:M/d/yyyy}");
        OpenUri($"github.com/SubnauticaNitrox/Nitrox/graphs/contributors?from={fromValue}&to={toValue}");
    }

    /// <summary>
    ///     Launches the server and Subnautica immediately if instant launch is active.
    /// </summary>
    [Conditional("DEBUG")]
    private void HandleInstantLaunchForDevelopment()
    {
        if (hasInstantLaunched)
        {
            return;
        }
        hasInstantLaunched = true;
        if (App.InstantLaunch == null)
        {
            return;
        }
        Task.Run(async () =>
        {
            // Start the server
            ServerEntry? server = await serverService.GetOrCreateServerAsync(App.InstantLaunch.SaveName);
            if (server == null)
            {
                throw new Exception("Failed to create new server save files");
            }
            server.Name = App.InstantLaunch.SaveName;
            Task serverStartTask = Dispatcher.UIThread.InvokeAsync(async () => await serverService.StartServerAsync(server)).ContinueWithHandleError();

            // Start a game in multiplayer for each player
            foreach (string playerName in App.InstantLaunch.PlayerNames)
            {
                await StartMultiplayerAsync(["--instantlaunch", playerName]).ContinueWithHandleError();
            }

            await serverStartTask;
        }).ContinueWithHandleError();
    }

    private async Task StartSubnauticaAsync(string[]? args = null) => await StartGameAsync(GameInfo.Subnautica, args);

    private async Task StartGameAsync(GameInfo gameInfo, string[]? args = null)
    {
        LauncherNotifier.Info("Starting game");

        string gameExePathSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "MacOS" : string.Empty;
        string gameExePath = Path.Combine(NitroxUser.GamePath, gameExePathSuffix, gameInfo.ExeName);
        if (!File.Exists(gameExePath))
        {
            throw new FileNotFoundException($"Unable to find {gameInfo.ExeName}");
        }

        // Start game & gaming platform if needed.
        string launchArguments = $"{keyValueStore.GetLaunchArguments(gameInfo)} {string.Join(" ", args ?? NitroxEnvironment.CommandLineArgs)}";

        if (!keyValueStore.GetIsDiscordIntegrationEnabled() && !launchArguments.Contains(DISABLE_DISCORD_INTEGRATION_ARG, StringComparison.OrdinalIgnoreCase))
        {
            launchArguments = $"{launchArguments} {DISABLE_DISCORD_INTEGRATION_ARG}";
        }

        ProcessEx game = NitroxUser.GamePlatform switch
        {
            Steam => await Steam.StartGameAsync(gameExePath, launchArguments, gameInfo.SteamAppId, ShouldSkipSteam(launchArguments), keyValueStore.GetUseBigPictureMode()),
            EpicGames => await EpicGames.StartGameAsync(gameExePath, launchArguments),
            HeroicGames => await HeroicGames.StartGameAsync(gameInfo.EgsNamespace, launchArguments),
            MSStore => await MSStore.StartGameAsync(gameExePath, launchArguments),
            Discord => await Discord.StartGameAsync(gameExePath, launchArguments),
            _ => await Standalone.StartGameAsync(gameExePath, launchArguments),
        };

        if (game is null)
        {
            throw new Exception($"Game failed to start through {NitroxUser.GamePlatform?.Name ?? "Standalone"}");
        }

        if (!launchArguments.Contains(DISABLE_DISCORD_INTEGRATION_ARG, StringComparison.OrdinalIgnoreCase))
        {
            _ = MonitorDiscordCrashFallbackAsync(args, game).ContinueWithHandleError();
        }
    }

    private async Task MonitorDiscordCrashFallbackAsync(string[]? args, ProcessEx game)
    {
        int processId = game.Id;
        DateTime startedAt = DateTime.UtcNow;

        Log.Info($"Monitoring Subnautica process #{processId} for Discord SDK launch crash for {DiscordCrashObservationPeriod.TotalSeconds:0} seconds");

        try
        {
            while (DateTime.UtcNow - startedAt < DiscordCrashObservationPeriod)
            {
                await Task.Delay(1000);

                if (!game.IsRunning)
                {
                    Log.Warn($"Subnautica process #{processId} exited during Discord crash observation window");

                    if (PlayerLogContainsDiscordCrashSignature(startedAt))
                    {
                        Log.Warn("Detected Discord SDK crash signature in Player.log. Relaunching with Discord integration disabled.");

                        LauncherNotifier.Warning("Subnautica appeared to crash while initializing Discord integration. Nitrox is relaunching with Discord integration disabled.");

                        string[] retryArgs = AppendDisableDiscordIntegrationArg(args);
                        await StartSubnauticaAsync(retryArgs);
                    }

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while monitoring for Discord SDK crash fallback:");
        }
        finally
        {
            game.Dispose();
        }
    }

    private static string[] AppendDisableDiscordIntegrationArg(string[]? args)
    {
        if (args?.Any(arg => arg.Equals(DISABLE_DISCORD_INTEGRATION_ARG, StringComparison.OrdinalIgnoreCase)) == true)
        {
            return args;
        }

        return [.. args ?? [], DISABLE_DISCORD_INTEGRATION_ARG];
    }

    private static bool PlayerLogContainsDiscordCrashSignature(DateTime launchMonitorStartedAt)
    {
        string playerLogPath = GetSubnauticaPlayerLogPath();

        if (!File.Exists(playerLogPath))
        {
            Log.Warn($"Unable to inspect Player.log for Discord crash signature because it does not exist: {playerLogPath}");
            return false;
        }

        DateTime playerLogLastWriteTime = File.GetLastWriteTimeUtc(playerLogPath);
        if (playerLogLastWriteTime < launchMonitorStartedAt.AddSeconds(-10))
        {
            Log.Info($"Ignoring Player.log for Discord crash detection because it was not updated during this launch attempt: {playerLogPath}");
            return false;
        }

        string playerLog = File.ReadAllText(playerLogPath);

        bool hasDiscordCrashSignature = DiscordCrashSignatures.Any(signature => playerLog.Contains(signature, StringComparison.OrdinalIgnoreCase));

        Log.Info($"Discord crash signature detected in Player.log: {hasDiscordCrashSignature}");

        return hasDiscordCrashSignature;
    }

    private static string GetSubnauticaPlayerLogPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Low",
                "Unknown Worlds",
                "Subnautica",
                "Player.log");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library",
                "Logs",
                "Unknown Worlds",
                "Subnautica",
                "Player.log");
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "unity3d",
            "Unknown Worlds",
            "Subnautica",
            "Player.log");
    }

    private bool ShouldSkipSteam(string args)
    {
        // Check if Steam overlay is enabled by user setting
        if (keyValueStore.GetUseBigPictureMode())
        {
            return false; // Use Steam if overlay is enabled
        }

        if (App.InstantLaunch != null)
        {
            // Running through Steam is fine if single instance.
            return App.InstantLaunch is { PlayerNames.Length: > 1 };
        }
        if (args.Contains("-vrmode none", StringComparison.OrdinalIgnoreCase))
        {
            if (keyValueStore.GetIsMultipleGameInstancesAllowed())
            {
                return true;
            }
        }
        else if (args.Contains("-vrmode", StringComparison.OrdinalIgnoreCase))
        {
            // VR Mode. Can only work if NOT going through Steam as it will always add '-vrmode none' due to hard coded default Steam "launch option" args. See: https://steamdb.info/app/264710/config/
            return true;
        }

        return false; // Default: use Steam unless explicitly disabled for special cases
    }
}
