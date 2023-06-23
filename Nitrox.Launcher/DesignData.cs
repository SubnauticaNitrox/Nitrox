using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;

namespace Nitrox.Launcher;

/// <summary>
///     Design-time data for use with the XAML previewer plugin.
/// </summary>
public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } = new();
    public static CreateServerViewModel CreateServerViewModel { get; } = new() { Name = "My Server Name", SelectedGameMode = GameMode.CREATIVE };
    public static ManageServerViewModel ManageServerViewModel { get; } = new(null) { Server = new ServerEntry { Name = "My fun server" } };
}
