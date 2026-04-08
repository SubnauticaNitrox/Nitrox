using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Launcher.ViewModels.Designer;

internal sealed class DesignLibraryViewModel : LibraryViewModel
{

    public DesignLibraryViewModel() : base(null!)
    {
        SelectedGame = new() { PathToGame = @"C:\Games\Steam\Subnautica", Platform = Platform.STEAM };

        LibraryEntries =
        [
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

        RecentServers =
        [
            new RecentServerEntry
            {
                ServerName = "nitrox.subnautica-server.net",
                PlayerCount = 67,
                IsOnline = true
            },
            new RecentServerEntry
            {
                ServerName = "server.nitrox-srv.net"
            },
            new RecentServerEntry
            {
                ServerName = "192.168.0.21",
                PlayerCount = 5,
                MaxPlayers = 6,
                IsOnline = true
            },
            new RecentServerEntry
            {
                ServerName = "0.0.0.0",
                IsOnline = true,
                PlayerCount = 1
            }
        ];
    }

}
