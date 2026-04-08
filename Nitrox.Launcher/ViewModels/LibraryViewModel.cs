using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Serialization;

namespace Nitrox.Launcher.ViewModels;

internal partial class LibraryViewModel(IKeyValueStore keyValueStore) : RoutableViewModelBase
{
    private readonly IKeyValueStore keyValueStore = keyValueStore;

    [ObservableProperty]
    public partial KnownGame SelectedGame { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<KnownGame>? LibraryEntries { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<RecentServerEntry>? RecentServers { get; set; }

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        //SelectedGame = new() { PathToGame = NitroxUser.GamePath, Platform = NitroxUser.GamePlatform?.Platform ?? Platform.NONE };
        //LibraryEntries = GetInstalledGames();

        // TODO: Implement installed game manager, then fetch LibraryEntries from it through GetInstalledGames()
        SelectedGame = new() { PathToGame = @"C:\Games\Steam\Subnautica", Platform = Platform.STEAM };
        LibraryEntries = [
            new KnownGame
            {
                PathToGame = @"C:\Games\Steam\Subnautica",
                Platform = Platform.STEAM
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\Epic\Subnautica",
                Platform = Platform.EPIC
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\MicrosoftStore\Subnautica",
                Platform = Platform.MICROSOFT
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\HeroicGames\Subnautica",
                Platform = Platform.HEROIC
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\Discord\Subnautica",
                Platform = Platform.DISCORD
            },
            new KnownGame
            {
                PathToGame = @"C:\Games\UhOh\Subnautica",
                Platform = Platform.NONE
            }
        ];

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
        // TODO: Change the selected game using a function in the installed game manager when implemented
        SelectedGame = game;
    }

}
