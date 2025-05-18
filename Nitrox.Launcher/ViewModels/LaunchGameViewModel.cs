using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
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

public partial class LaunchGameViewModel : RoutableViewModelBase
{
    public static Task<string> LastFindSubnauticaTask;
    private static bool hasInstantLaunched;

    private readonly OptionsViewModel optionsViewModel;
    private readonly ServerService serverService;
    private readonly IKeyValueStore keyValueStore;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private Platform gamePlatform;

    [ObservableProperty]
    private string platformToolTip;

    public Bitmap[] GalleryImageSources { get; } = [
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-1.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-2.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-3.png", static stream => new Bitmap(stream)),
        AssetHelper.GetAssetFromStream("/Assets/Images/gallery/image-4.png", static stream => new Bitmap(stream))
    ];

    public string Version => $"{NitroxEnvironment.ReleasePhase} {NitroxEnvironment.Version}";
    public string SubnauticaLaunchArguments => keyValueStore.GetSubnauticaLaunchArguments();

    public LaunchGameViewModel()
    {
    }

    public LaunchGameViewModel(IDialogService dialogService, ServerService serverService, OptionsViewModel optionsViewModel, IKeyValueStore keyValueStore)
    {
        this.dialogService = dialogService;
        this.serverService = serverService;
        this.optionsViewModel = optionsViewModel;
        this.keyValueStore = keyValueStore;
    }

    internal override async Task ViewContentLoadAsync()
    {
        await Task.Run(() =>
        {
            NitroxUser.GamePlatformChanged += UpdateGamePlatform;
            UpdateGamePlatform();
            HandleInstantLaunchForDevelopment();
        });
    }

    internal override Task ViewContentUnloadAsync()
    {
        NitroxUser.GamePlatformChanged -= UpdateGamePlatform;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartSingleplayerAsync()
    {
        if (GameInspect.WarnIfGameProcessExists(GameInfo.Subnautica))
        {
            return;
        }

        Log.Info("Launching Subnautica in singleplayer mode");

        try
        {
            if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
            {
                await HostScreen.ShowAsync(optionsViewModel);
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

    [RelayCommand]
    private async Task StartMultiplayerAsync(string[] args = null)
    {
        Log.Info("Launching Subnautica in multiplayer mode");
        try
        {
            bool setupResult = await Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(NitroxUser.GamePath) || !Directory.Exists(NitroxUser.GamePath))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await HostScreen.ShowAsync(optionsViewModel));
                    LauncherNotifier.Warning("Location of Subnautica is unknown. Set the path to it in settings");
                    return false;
                }
                if (PirateDetection.HasTriggered)
                {
                    LauncherNotifier.Error("Aarrr! Nitrox has walked the plank :(");
                    return false;
                }
                if (GameInspect.WarnIfGameProcessExists(GameInfo.Subnautica))
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
                NitroxEntryPatch.Remove(NitroxUser.GamePath);
                NitroxEntryPatch.Apply(NitroxUser.GamePath);

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
            await Dispatcher.UIThread.InvokeAsync(async () => await dialogService.ShowErrorAsync(ex, "Error while starting game in multiplayer mode"));
        }
    }
    
    [RelayCommand]
    private void OpenContributionsOfYear()
    {
        Process.Start(new ProcessStartInfo($"https://github.com/SubnauticaNitrox/Nitrox/graphs/contributors?from={HttpUtility.UrlEncode($"{DateTime.UtcNow.AddYears(-1):yyyy/M/d}")}") { UseShellExecute = true, Verb = "open" })?.Dispose();
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
            ServerEntry server = await serverService.GetOrCreateServerAsync(App.InstantLaunch.SaveName);
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

    private async Task StartSubnauticaAsync(string[] args = null)
    {
        LauncherNotifier.Info("Starting game");
        string subnauticaPath = NitroxUser.GamePath;
        string subnauticaLaunchArguments = $"{SubnauticaLaunchArguments} {string.Join(" ", args ?? Environment.GetCommandLineArgs())}";
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
        
        IGamePlatform platform = GamePlatforms.GetPlatformByGameDir(subnauticaPath);

        // Start game & gaming platform if needed.
        using ProcessEx game = platform switch
        {
            Steam s => await s.StartGameAsync(subnauticaExe, subnauticaLaunchArguments, GameInfo.Subnautica.SteamAppId),
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

    private void UpdateGamePlatform()
    {
        GamePlatform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE;
        PlatformToolTip = GamePlatform.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";
    }
}
