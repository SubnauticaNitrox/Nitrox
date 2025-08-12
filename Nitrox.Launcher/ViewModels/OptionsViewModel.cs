using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Discovery;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.ViewModels;

public partial class OptionsViewModel : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetArgumentsCommand))]
    private string launchArgs;

    [ObservableProperty]
    private string savesFolderDir;

    [ObservableProperty]
    private KnownGame selectedGame;

    [ObservableProperty]
    private bool showResetArgsBtn;

    private static string DefaultLaunchArg => "-vrmode none";

    public OptionsViewModel()
    {
    }

    public OptionsViewModel(IKeyValueStore keyValueStore)
    {
        this.keyValueStore = keyValueStore;
    }

    internal override async Task ViewContentLoadAsync()
    {
        await Task.Run(() =>
        {
            SelectedGame = new() { PathToGame = NitroxUser.GamePath, Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE };
            LaunchArgs = keyValueStore.GetSubnauticaLaunchArguments(DefaultLaunchArg);
            SavesFolderDir = keyValueStore.GetSavesFolderDir();
        });
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
            if (NitroxEntryPatch.IsPatchApplied(NitroxUser.GamePath))
            {
                NitroxEntryPatch.Remove(NitroxUser.GamePath);
            }

            return path;
        });

        await LaunchGameViewModel.LastFindSubnauticaTask;
    }

    [RelayCommand]
    private async Task SetGamePath()
    {
        string selectedDirectory = await MainWindow.StorageProvider.OpenFolderPickerAsync("Select Subnautica installation directory", SelectedGame.PathToGame);
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
    private void ResetArguments(IInputElement focusTargetAfterReset = null)
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
    private void OpenSavesFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = SavesFolderDir,
            Verb = "open",
            UseShellExecute = true
        })?.Dispose();
    }
}
