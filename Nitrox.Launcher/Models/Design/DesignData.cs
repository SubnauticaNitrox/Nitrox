using System;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Discovery.Models;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Design-time data for use with the XAML previewer plugin.
/// </summary>
public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } = new(null);
    public static LaunchGameViewModel LaunchGameViewModel { get; } = new(null);
    public static ManageServerViewModel ManageServerViewModel { get; } = new(null) { ServerName = "My fun server" };
    public static CreateServerViewModel CreateServerViewModel { get; } = new() { Name = "My Server Name", SelectedGameMode = NitroxGameMode.CREATIVE };
    public static LibraryViewModel LibraryViewModel { get; } = new(null);
    public static CommunityViewModel CommunityViewModel { get; } = new(null);
    public static BlogViewModel BlogViewModel { get; } = new(null, [new NitroxBlog("Design blog", DateTime.UtcNow - TimeSpan.FromDays(5), "google.com", null)]);
    public static UpdatesViewModel UpdatesViewModel { get; } = new(null);

    public static OptionsViewModel OptionsViewModel { get; } = new(null)
    {
        SelectedGame = new()
        {
            PathToGame = @"C:\Games\Steam\steamapps\common\Subnautica",
            Platform = Platform.STEAM
        }
        
        //KnownGames =
        //[
        //    new OptionsViewModel.KnownGame
        //    {
        //        PathToGame = @"C:\Games\Steam\steamapps\common\Subnautica",
        //        Platform = Platform.STEAM
        //    },
        //    new OptionsViewModel.KnownGame
        //    {
        //        PathToGame = @"C:\Games\Epic\Subnautica",
        //        Platform = Platform.EPIC
        //    },
        //    new OptionsViewModel.KnownGame
        //    {
        //        PathToGame = @"C:\Games\Discord\Subnautica",
        //        Platform = Platform.DISCORD
        //    },
        //    new OptionsViewModel.KnownGame
        //    {
        //        PathToGame = @"C:\Gamepass\Subnautica",
        //        Platform = Platform.MICROSOFT
        //    }
        //]
    };

    public static ConfirmationBoxViewModel ConfirmationBoxViewModel { get; } = new() { ConfirmationText = "Confirmation Text" };
}
