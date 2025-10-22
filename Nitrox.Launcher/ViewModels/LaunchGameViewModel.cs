using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    private static bool hasInstantLaunched;
    private readonly DialogService dialogService = dialogService;
    private readonly IKeyValueStore keyValueStore = keyValueStore;

    private readonly ServerService serverService = serverService;

    [ObservableProperty]
    private Platform gamePlatform;

    [ObservableProperty]
    private string? platformToolTip;

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

                    string patcherDllPath = Path.Combine(NitroxUser.ExecutableRootPath ?? "", "lib", "net472", PATCHER_DLL_NAME);
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
        ProcessEx game = NitroxUser.GamePlatform switch
        {
            Steam => await Steam.StartGameAsync(gameExePath, launchArguments, gameInfo.SteamAppId, ShouldSkipSteam(launchArguments)),
            EpicGames => await EpicGames.StartGameAsync(gameExePath, launchArguments),
            HeroicGames => await HeroicGames.StartGameAsync(gameInfo.EgsNamespace, launchArguments),
            MSStore => await MSStore.StartGameAsync(gameExePath, launchArguments),
            Discord => await Discord.StartGameAsync(gameExePath, launchArguments),
            _ => throw new Exception($"Directory '{NitroxUser.GamePath}' is not a valid {gameInfo.Name} game installation or the game platform is unsupported by Nitrox.")
        };

        if (game is null)
        {
            throw new Exception($"Game failed to start through {NitroxUser.GamePlatform.Name}");
        }
    }

    private bool ShouldSkipSteam(string args)
    {
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

        return false;
    }
}
