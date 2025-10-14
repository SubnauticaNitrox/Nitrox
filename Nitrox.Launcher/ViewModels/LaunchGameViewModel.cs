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
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.Store;
using NitroxModel.Platforms.Store.Interfaces;

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
    public string SubnauticaLaunchArguments => keyValueStore.GetSubnauticaLaunchArguments();

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            NitroxUser.GamePlatformChanged += UpdateGamePlatform;
            UpdateGamePlatform();
            HandleInstantLaunchForDevelopment();
        }, cancellationToken);
    }

    internal override Task ViewContentUnloadAsync()
    {
        NitroxUser.GamePlatformChanged -= UpdateGamePlatform;
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

    private async Task StartSubnauticaAsync(string[]? args = null)
    {
        LauncherNotifier.Info("Starting game");
        string subnauticaPath = NitroxUser.GamePath;
        string subnauticaLaunchArguments = $"{SubnauticaLaunchArguments} {string.Join(" ", args ?? NitroxEnvironment.CommandLineArgs)}";
        string subnauticaExe;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            subnauticaExe = Path.Combine(subnauticaPath, "MacOS", GameInfo.Subnautica.ExeName);
        }
        else
        {
            subnauticaExe = Path.Combine(subnauticaPath, GameInfo.Subnautica.ExeName);
        }
        if (!File.Exists(subnauticaExe))
        {
            throw new FileNotFoundException("Unable to find Subnautica executable");
        }

        // Start game & gaming platform if needed.
        IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(subnauticaPath);
        
        // BIG PICTURE MODE: Two-stage launch process
        if (keyValueStore.GetIsBigPictureModeEnabled() && platform is Steam steamPlatform)
        {
            await LaunchWithBigPictureMode(steamPlatform, subnauticaExe, subnauticaLaunchArguments);
            return; // Exit early - Big Picture mode handles launch in background
        }

        using ProcessEx game = platform switch
        {
            Steam s => await s.StartGameAsync(subnauticaExe, subnauticaLaunchArguments, GameInfo.Subnautica.SteamAppId, ShouldSkipSteam(subnauticaLaunchArguments), false),
            EpicGames e => await e.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            MSStore m => await m.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            Discord d => await d.StartGameAsync(subnauticaExe, subnauticaLaunchArguments),
            _ => throw new Exception($"Directory '{subnauticaPath}' is not a valid {GameInfo.Subnautica.Name} game installation or the game platform is unsupported by Nitrox.")
        };

        if (game is null)
        {
            throw new Exception($"Game failed to start through {platform.Name}");
        }
    }

    /// <summary>
    /// STEAM OVERLAY INTEGRATION:
    /// Determines whether to launch game through Steam or directly.
    /// Steam launch enables overlay, Steam Input, controller support, and OSK functionality.
    /// Critical for Steam Deck and controller users who rely on Steam Input for navigation and text entry.
    /// </summary>
    private bool ShouldSkipSteam(string args)
    {
        // Big Picture mode always forces Steam launch when enabled
        if (keyValueStore.GetIsBigPictureModeEnabled())
        {
            return false; // Always use Steam for Big Picture mode
        }

        // Check if Steam overlay is disabled by user setting
        if (!keyValueStore.GetIsSteamOverlayEnabled())
        {
            return true; // Skip Steam if overlay is disabled
        }

        // Check if game is actually from Steam before forcing overlay
        if (!IsGameFromSteam())
        {
            return true; // Skip Steam if game isn't from Steam
        }

        // Force Steam launch for better controller and overlay support (Steam Deck, controllers, OSK)
        // Check if we're on a handheld device or Steam Deck
        if (IsHandheldOrControllerPreferred())
        {
            // Always use Steam for Steam Deck and controller users to enable overlay and Steam Input
            return false;
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

        return false;
    }

    /// <summary>
    /// HANDHELD/CONTROLLER DETECTION:
    /// Detects Steam Deck, Big Picture mode, and other handheld gaming devices where controller input is preferred.
    /// These devices benefit most from Steam overlay and Steam Input functionality.
    /// Used to automatically prefer Steam launch for better user experience on handheld devices.
    /// </summary>
    private bool IsHandheldOrControllerPreferred()
    {
        // Big Picture mode explicitly indicates controller/TV preference
        if (keyValueStore.GetIsBigPictureModeEnabled())
        {
            return true;
        }

        // Check for Steam Deck environment
        return Environment.GetEnvironmentVariable("SteamDeck") != null ||
               File.Exists("/home/deck/.steampid") ||
               Environment.GetEnvironmentVariable("STEAM_DECK") != null ||
               // Check if running under Steam (which usually indicates controller preference)
               Environment.GetEnvironmentVariable("SteamAppId") != null ||
               Environment.GetEnvironmentVariable("SteamGameId") != null;
    }

    private bool IsGameFromSteam()
    {
        string gamePath = NitroxUser.GamePath;
        
        // Check if the game path contains Steam-specific directory indicators
        if (string.IsNullOrEmpty(gamePath))
        {
            return false;
        }

        // Check for common Steam directory patterns
        return gamePath.Contains("steamapps", StringComparison.OrdinalIgnoreCase) ||
               gamePath.Contains("Steam", StringComparison.OrdinalIgnoreCase) ||
               // Check for Steam API DLL files in game directory
               File.Exists(Path.Combine(gamePath, GameInfo.Subnautica.DataFolder, "Plugins", "x86_64", "steam_api64.dll")) ||
               File.Exists(Path.Combine(gamePath, GameInfo.Subnautica.DataFolder, "Plugins", "steam_api64.dll")) ||
               File.Exists(Path.Combine(gamePath, "steam_appid.txt"));
    }

    /// <summary>
    /// BIG PICTURE MODE ENHANCED LAUNCH:
    /// Implements two-stage launch process for Big Picture mode.
    /// Stage 1: Launch Steam Big Picture mode and wait for it to fully load
    /// Stage 2: Launch Subnautica through Steam while keeping launcher in background
    /// This ensures Big Picture interface is ready before game launch.
    /// </summary>
    private async Task LaunchWithBigPictureMode(Steam steamPlatform, string subnauticaExe, string launchArguments)
    {
        try
        {
            // Stage 1: Launch Steam Big Picture Mode
            Log.Info("Big Picture Mode: Launching Steam Big Picture interface...");
            
            string? steamExe = steamPlatform.GetExeFile();
                
            if (steamExe == null)
            {
                throw new Exception("Could not find Steam executable for Big Picture mode launch");
            }

            // Launch Steam Big Picture Mode - Multiple methods to ensure it activates
            bool bigPictureStarted = false;
            
            // Method 1: Try Steam protocol URL
            try
            {
                ProcessStartInfo protocolStart = new()
                {
                    FileName = "steam://open/bigpicture",
                    UseShellExecute = true
                };
                Process.Start(protocolStart);
                Log.Info("Big Picture Mode: Attempted Steam protocol activation...");
                await Task.Delay(1000); // Give protocol time to work
                bigPictureStarted = true;
            }
            catch (Exception ex)
            {
                Log.Info($"Steam protocol failed: {ex.Message}, trying direct launch...");
            }

            // Method 2: If protocol failed, try direct Steam command
            if (!bigPictureStarted)
            {
                ProcessStartInfo directStart = new()
                {
                    FileName = steamExe,
                    Arguments = "-bigpicture -tenfoot", // Multiple flags for Big Picture mode
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(directStart);
                Log.Info("Big Picture Mode: Launched Steam with Big Picture flags...");
            }

            // Stage 2: Wait for Big Picture to fully load, then launch game
            // Allow configurable delay - default 5 seconds should be enough for most systems
            int delayMs = keyValueStore.GetValue("BigPictureLaunchDelay", 5000);
            Log.Info($"Big Picture Mode: Waiting {delayMs/1000} seconds for interface to load...");
            
            await Task.Delay(delayMs);
            
            Log.Info("Big Picture Mode: Launching Subnautica through Steam in background...");
            
            // Launch game through Steam while Big Picture stays active
            ProcessStartInfo gameStart = new()
            {
                FileName = steamExe,
                Arguments = $@"-applaunch {GameInfo.Subnautica.SteamAppId} --nitrox ""{NitroxUser.LauncherPath}"" {launchArguments}",
                UseShellExecute = false,
                CreateNoWindow = true // Keep launcher in background
            };

            Process.Start(gameStart);
            Log.Info("Big Picture Mode: Subnautica launched successfully through Steam");
            
            // Minimize launcher window to background but keep it running for monitoring
            // The launcher stays active to handle any Nitrox coordination
        }
        catch (Exception ex)
        {
            Log.Error($"Big Picture Mode launch failed: {ex.Message}");
            throw;
        }
    }

    private void UpdateGamePlatform()
    {
        GamePlatform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
        PlatformToolTip = GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "";
    }
}
