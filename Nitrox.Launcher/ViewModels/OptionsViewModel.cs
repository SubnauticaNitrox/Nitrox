using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.ViewModels;

internal partial class OptionsViewModel(IKeyValueStore keyValueStore, StorageService storageService) : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore = keyValueStore;
    private readonly StorageService storageService = storageService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetArgumentsCommand))]
    private string launchArgs;

    [ObservableProperty]
    private string programDataFolderDir;

    [ObservableProperty]
    private string screenshotsFolderDir;

    [ObservableProperty]
    private string savesFolderDir;

    [ObservableProperty]
    private string logsFolderDir;

    [ObservableProperty]
    private KnownGame selectedGame;

    [ObservableProperty]
    private bool showResetArgsBtn;

    [ObservableProperty]
    private bool lightModeEnabled;

    [ObservableProperty]
    private bool allowMultipleGameInstances;

    [ObservableProperty]
    private bool useBigPictureMode;
    
    [ObservableProperty]
    private bool isInReleaseMode;

    private static string DefaultLaunchArg => "-vrmode none";
    private bool isResettingArgs;

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        SelectedGame = new() { PathToGame = NitroxUser.GamePath, Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE };
        LaunchArgs = keyValueStore.GetLaunchArguments(GameInfo.Subnautica, DefaultLaunchArg);
        ProgramDataFolderDir = NitroxUser.AppDataPath;
        ScreenshotsFolderDir = NitroxUser.ScreenshotsPath;
        SavesFolderDir = keyValueStore.GetSavesFolderDir();
        LogsFolderDir = Model.Logger.Log.LogDirectory;
        LightModeEnabled = keyValueStore.GetIsLightModeEnabled();
        AllowMultipleGameInstances = keyValueStore.GetIsMultipleGameInstancesAllowed();
        UseBigPictureMode = keyValueStore.GetUseBigPictureMode();
        IsInReleaseMode = NitroxEnvironment.IsReleaseMode;
        await SetTargetedSubnauticaPathAsync(SelectedGame.PathToGame).ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));
    }

    public async Task SetTargetedSubnauticaPathAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        if (LaunchGameViewModel.LastFindSubnauticaTask != null)
        {
            await LaunchGameViewModel.LastFindSubnauticaTask;
        }

        LaunchGameViewModel.LastFindSubnauticaTask = Task.Run(() =>
        {
            PirateDetection.TriggerOnDirectory(path);

            if (!FileSystem.Instance.IsWritable(Directory.GetCurrentDirectory()) || !FileSystem.Instance.IsWritable(path))
            {
                // TODO: Move this check to another place where Nitrox installation can be verified. (i.e: another page on the launcher in order to check permissions, network setup, ...)
                if (!FileSystem.Instance.SetFullAccessToCurrentUser(Directory.GetCurrentDirectory()) || !FileSystem.Instance.SetFullAccessToCurrentUser(path))
                {
                    LauncherNotifier.Error("Restart Nitrox Launcher as admin to allow Nitrox to change permissions as needed. This is only needed once. Nitrox will close after this message.");
                    return null;
                }
            }

            // Save game path as preferred for future sessions.
            NitroxUser.PreferredGamePath = path;

            return path;
        });

        await LaunchGameViewModel.LastFindSubnauticaTask;
    }

    [RelayCommand]
    private async Task SetGamePath()
    {
        string selectedDirectory = await storageService.OpenFolderPickerAsync("Select Subnautica installation directory", SelectedGame.PathToGame);
        if (selectedDirectory == "")
        {
            return;
        }

        if (!GameInstallationHelper.HasGameExecutable(selectedDirectory, GameInfo.Subnautica))
        {
            LauncherNotifier.Error("Invalid subnautica directory");
            return;
        }

        if (!selectedDirectory.Equals(SelectedGame.PathToGame, StringComparison.OrdinalIgnoreCase))
        {
            await SetTargetedSubnauticaPathAsync(selectedDirectory);
            SelectedGame = new() { PathToGame = NitroxUser.GamePath, Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE };
            LauncherNotifier.Success("Applied changes");
        }
    }

    [RelayCommand]
    private void ResetArguments(IInputElement? focusTargetAfterReset = null)
    {
        isResettingArgs = true;
        LaunchArgs = DefaultLaunchArg;
        SetArguments();
        isResettingArgs = false;

        focusTargetAfterReset?.Focus();
    }

    [RelayCommand(CanExecute = nameof(CanSetArguments))]
    private void SetArguments()
    {
        keyValueStore.SetLaunchArguments(GameInfo.Subnautica, LaunchArgs);
        SetArgumentsCommand.NotifyCanExecuteChanged();
    }

    private bool CanSetArguments()
    {
        ShowResetArgsBtn = LaunchArgs != DefaultLaunchArg;

        return LaunchArgs != keyValueStore.GetLaunchArguments(GameInfo.Subnautica, DefaultLaunchArg) && !isResettingArgs;
    }

    [RelayCommand]
    private void DisplaySteamOverlayNotification()
    {
        if (AllowMultipleGameInstances && SelectedGame.Platform == Platform.STEAM)
        {
            LauncherNotifier.Warning("Note: Enabling this option will disable Steam's in-game overlay. Disable this option to use Steam's overlay");
        }
    }

    [RelayCommand]
    private void OpenFolder(string? dir = null)
    {
        try
        {
            if (!OpenDirectory(dir))
            {
                LauncherNotifier.Error("Can't open. Directory does not exist.");
            }
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error($"Failed to open folder: {ex.Message}");
        }
    }

    partial void OnLightModeEnabledChanged(bool value)
    {
        keyValueStore.SetIsLightModeEnabled(value);
        Dispatcher.UIThread.Invoke(() => Application.Current!.RequestedThemeVariant = value ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    partial void OnAllowMultipleGameInstancesChanged(bool value)
    {
        if (value)
        {
            UseBigPictureMode = false;
        }
        keyValueStore.SetIsMultipleGameInstancesAllowed(value);
    }
    
    partial void OnUseBigPictureModeChanged(bool value)
    {
        if (value)
        {
            AllowMultipleGameInstances = false;
        }
        keyValueStore.SetBigPictureMode(value);
    }
}
