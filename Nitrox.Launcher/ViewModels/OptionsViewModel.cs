using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
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
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Launcher.ViewModels;

internal partial class OptionsViewModel(GameInstallationService gameInstallationService, IKeyValueStore keyValueStore, StorageService storageService, DialogService dialogService) : RoutableViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetArgumentsCommand))]
    public partial string LaunchArgs { get; set; }

    [ObservableProperty]
    public partial string ProgramDataPath { get; set; }

    [ObservableProperty]
    public partial string ScreenshotsPath { get; set; }

    [ObservableProperty]
    public partial string SavesPath { get; set; }

    [ObservableProperty]
    public partial string LogsPath { get; set; }

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<KnownGame> KnownGames { get; set; }

    [ObservableProperty]
    public partial bool ShowResetArgsBtn { get; set; }

    [ObservableProperty]
    public partial bool LightModeEnabled { get; set; }

    [ObservableProperty]
    public partial bool AllowMultipleGameInstances { get; set; }

    [ObservableProperty]
    public partial bool UseBigPictureMode { get; set; }    

    [ObservableProperty]
    public partial bool IsInReleaseMode { get; set; }
    
    private static string DefaultLaunchArg => "-vrmode none";
    private bool isResettingArgs;

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        SelectedGame = gameInstallationService.SelectedGame;
        KnownGames = gameInstallationService.InstalledGames;
        LaunchArgs = keyValueStore.GetLaunchArguments(GameInfo.Subnautica, DefaultLaunchArg);
        ProgramDataPath = NitroxUser.AppDataPath;
        ScreenshotsPath = NitroxUser.ScreenshotsPath;
        SavesPath = keyValueStore.GetSavesFolderDir();
        LogsPath = Model.Logger.Log.LogDirectory;
        LightModeEnabled = keyValueStore.GetIsLightModeEnabled();
        AllowMultipleGameInstances = keyValueStore.GetIsMultipleGameInstancesAllowed();
        UseBigPictureMode = keyValueStore.GetUseBigPictureMode();
        IsInReleaseMode = NitroxEnvironment.IsReleaseMode;
    }

    [RelayCommand]
    private async Task AddGameInstallation()
    {
        string selectedDirectory = await storageService.OpenFolderPickerAsync("Select Subnautica installation directory", SelectedGame.PathToGame);
        if (string.IsNullOrWhiteSpace(selectedDirectory))
        {
            return;
        }

        if (selectedDirectory.Equals(SelectedGame.PathToGame, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string? errorMessage = null;
        bool added = await Task.Run(() => gameInstallationService.AddGameInstallation(GameInfo.Subnautica, selectedDirectory, out errorMessage));
        if (!added)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                LauncherNotifier.Error(errorMessage);
            }
            return;
        }

        SelectedGame = gameInstallationService.SelectedGame;
        LauncherNotifier.Success("Added game installation");
    }

    [RelayCommand]
    private void SetSelectedGame(KnownGame game)
    {
        gameInstallationService.SelectGameInstallation(GameInfo.Subnautica, game);
        SelectedGame = gameInstallationService.SelectedGame;
    }

    [RelayCommand]
    private async Task RemoveGameInstallation(KnownGame game)
    {
        if (game == null || string.IsNullOrWhiteSpace(game.PathToGame))
        {
            return;
        }

        DialogBoxViewModel confirmResult = await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Title = $"Are you sure you want to remove the game installation '{game.PathToGame}'?";
            model.Description = "This will remove the installation from the launcher cache and it will no longer appear in the installation list unless it is added again.";
            model.ButtonOptions = ButtonOptions.YesNo;
        });

        if (!confirmResult)
        {
            return;
        }

        if (!gameInstallationService.RemoveGameInstallation(GameInfo.Subnautica, game))
        {
            LauncherNotifier.Error("Failed to remove game installation");
            return;
        }

        SelectedGame = gameInstallationService.SelectedGame;
        LauncherNotifier.Success("Game installation removed");
    }

    [RelayCommand]
    private void RefreshGameInstallations()
    {
        gameInstallationService.RefreshInstalledGames(GameInfo.Subnautica);
        LauncherNotifier.Success("Refreshed game installations");
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
