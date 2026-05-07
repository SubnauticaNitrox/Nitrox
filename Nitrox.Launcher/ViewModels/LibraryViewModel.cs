using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Logger;

namespace Nitrox.Launcher.ViewModels;

internal partial class LibraryViewModel(GameInstallationService gameInstallationService, StorageService storageService, DialogService dialogService, RecentServerStatusService recentServerStatusService) : RoutableViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveGameInstallationCommand))]
    public partial KnownGame SelectedGame { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<KnownGame> LibraryEntries { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshRecentServersCommand))]
    public partial AvaloniaList<RecentServerEntry> RecentServers { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshRecentServersCommand))]
    public partial bool IsRefreshingRecentServers { get; set; }

    internal override Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        SelectedGame = gameInstallationService.SelectedGame;
        LibraryEntries = gameInstallationService.InstalledGames;

        RecentServers = recentServerStatusService.GetRecentServers();

        AvaloniaList<RecentServerEntry> serversToRefresh = recentServerStatusService.GetServersToRefreshOnLoad(RecentServers);
        _ = recentServerStatusService.RefreshRecentServersAsync(serversToRefresh, false, cancellationToken).ContinueWithHandleError();

        recentServerStatusService.StartAutoRefresh(RecentServers, cancellationToken);
        return Task.CompletedTask;
    }

    internal override Task ViewContentUnloadAsync()
    {
        recentServerStatusService.SaveOnlineServersState(RecentServers);
        recentServerStatusService.StopAutoRefresh();

        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanRefreshRecentServers), AllowConcurrentExecutions = false)]
    private async Task RefreshRecentServers()
    {
        try
        {
            IsRefreshingRecentServers = true;
            await recentServerStatusService.RefreshRecentServersAsync(RecentServers, true, CancellationToken.None);
            LauncherNotifier.Success("Refreshed recent servers");
        }
        catch (OperationCanceledException)
        {
            LauncherNotifier.Warning("Timed out while refreshing recent server status");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh recent server status");
            LauncherNotifier.Error("Failed to refresh recent server status");
        }
        finally
        {
            IsRefreshingRecentServers = false;
        }
    }

    private bool CanRefreshRecentServers() => !IsRefreshingRecentServers && RecentServers.Count > 0;

    [RelayCommand]
    private void SetSelectedGame(KnownGame game)
    {
        gameInstallationService.SelectGameInstallation(GameInfo.Subnautica, game);
        SelectedGame = gameInstallationService.SelectedGame;
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

    [RelayCommand(CanExecute = nameof(CanRemoveGameInstallation))]
    private async Task RemoveGameInstallation(KnownGame? game)
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
    private async Task RefreshGameInstallations()
    {
        await gameInstallationService.RefreshInstalledGamesAsync(GameInfo.Subnautica);
        SelectedGame = gameInstallationService.SelectedGame;
        LauncherNotifier.Success("Refreshed game installations");
    }

    private bool CanRemoveGameInstallation(KnownGame? game)
    {
        return game != null && !string.IsNullOrWhiteSpace(game.PathToGame);
    }

}
