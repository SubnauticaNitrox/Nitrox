using Nitrox.Launcher.ViewModels;
using NitroxModel.Server;

namespace Nitrox.Launcher;

/// <summary>
///     Design-time data for use with the XAML previewer plugin.
/// </summary>
public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } = new(null);
    public static CreateServerViewModel CreateServerViewModel { get; } = new() { Name = "My Server Name", SelectedGameMode = ServerGameMode.CREATIVE };
    public static ManageServerViewModel ManageServerViewModel { get; } = new(null) { ServerName = "My fun server" };
    public static ConfirmationBoxViewModel ConfirmationBoxViewModel { get; } = new() { ConfirmationText = "Confirmation Text" };
}
