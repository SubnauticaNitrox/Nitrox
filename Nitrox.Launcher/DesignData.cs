using Nitrox.Launcher.ViewModels;
using NitroxModel.Server;

namespace Nitrox.Launcher;

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
    public static BlogViewModel BlogViewModel { get; } = new(null);
    public static UpdatesViewModel UpdatesViewModel { get; } = new(null);
    public static OptionsViewModel OptionsViewModel { get; } = new(null);
    public static ConfirmationBoxViewModel ConfirmationBoxViewModel { get; } = new() { ConfirmationText = "Confirmation Text" };
}
