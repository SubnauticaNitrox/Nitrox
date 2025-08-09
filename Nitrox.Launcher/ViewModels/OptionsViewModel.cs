using System;
using System.Diagnostics;
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
using NitroxModel.Discovery;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.ViewModels;

internal partial class OptionsViewModel(IKeyValueStore keyValueStore, StorageService storageService) : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore = keyValueStore;
    private readonly StorageService storageService = storageService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetArgumentsCommand))]
    private string launchArgs;

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
    private bool isInReleaseMode;
    
    [ObservableProperty]
    private string multipleInstancesTooltip;

    private static string DefaultLaunchArg => "-vrmode none";

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            SelectedGame = new() { PathToGame = NitroxUser.GamePath, Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE };
            LaunchArgs = keyValueStore.GetSubnauticaLaunchArguments(DefaultLaunchArg);
            LogsFolderDir = NitroxModel.Logger.Log.LogDirectory;
            LightModeEnabled = keyValueStore.GetIsLightModeEnabled();
            AllowMultipleGameInstances = keyValueStore.GetIsMultipleGameInstancesAllowed();
            IsInReleaseMode = NitroxEnvironment.IsReleaseMode;
        }, cancellationToken);
        await SetTargetedSubnauticaPathAsync(SelectedGame.PathToGame).ContinueWithHandleError(ex => LauncherNotifier.Error(ex.Message));
    }

    public async Task SetTargetedSubnauticaPathAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        NitroxUser.GamePath = path;
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
        LaunchArgs = DefaultLaunchArg;
        ShowResetArgsBtn = false;
        SetArgumentsCommand.NotifyCanExecuteChanged();
        focusTargetAfterReset?.Focus();
    }

    [RelayCommand(CanExecute = nameof(CanSetArguments))]
    private void SetArguments()
    {
        keyValueStore.SetSubnauticaLaunchArguments(LaunchArgs);
        SetArgumentsCommand.NotifyCanExecuteChanged();
    }

    private bool CanSetArguments()
    {
        ShowResetArgsBtn = LaunchArgs != DefaultLaunchArg;

        return LaunchArgs != keyValueStore.GetSubnauticaLaunchArguments(DefaultLaunchArg);
    }

    [RelayCommand]
    private void OpenLogsFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = LogsFolderDir,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }
    
    partial void OnLightModeEnabledChanged(bool value)
    {
        keyValueStore.SetIsLightModeEnabled(value);
        Dispatcher.UIThread.Invoke(() => Application.Current!.RequestedThemeVariant = value ? ThemeVariant.Light : ThemeVariant.Dark);
    }
    
    partial void OnAllowMultipleGameInstancesChanged(bool value)
    {
        keyValueStore.SetIsMultipleGameInstancesAllowed(value);
    }
}
