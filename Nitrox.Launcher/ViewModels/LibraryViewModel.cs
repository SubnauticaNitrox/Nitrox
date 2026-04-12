using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Serialization;

namespace Nitrox.Launcher.ViewModels;

internal partial class LibraryViewModel(GameInstallationService gameInstallationService) : RoutableViewModelBase
{
    private readonly GameInstallationService gameInstallationService = gameInstallationService;

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<KnownGame> LibraryEntries { get; set; } = [];

    [ObservableProperty]
    public partial AvaloniaList<RecentServerEntry>? RecentServers { get; set; }

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        SelectedGame = gameInstallationService.SelectedGame;
        LibraryEntries = gameInstallationService.InstalledGames;
        RecentServers = GetRecentServers(); // TODO: Make Async and await this?

    }

    private AvaloniaList<RecentServerEntry> GetRecentServers()
    {
        AvaloniaList<RecentServerEntry> list = [];

        ServerList.Refresh();
        foreach (ServerList.Entry entry in ServerList.Instance.Entries)
        {
            list.Add(new RecentServerEntry
            {
                ServerName = entry.Name,
                ServerIP = entry.Address,
                ServerPort = entry.Port
            });
        }

        return list;
    }

    [RelayCommand]
    private void SetSelectedGame(KnownGame game)
    {
        gameInstallationService.SelectGameInstallation(game);
        SelectedGame = gameInstallationService.SelectedGame;
    }

}
