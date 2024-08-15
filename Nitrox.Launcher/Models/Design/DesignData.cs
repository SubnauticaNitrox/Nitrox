using System;
using Avalonia.Controls.Notifications;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Discovery.Models;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Design-time data for use with the XAML previewer plugin.
/// </summary>
public static class DesignData
{
    static DesignData()
    {
        // Skip initialization if not in design mode.
        if (!Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }

        try
        {
            MainWindowViewModel = new(null, null, null, null, null, null, null, null, notifications: [new NotificationItem("Something bad happened :(", NotificationType.Error), new NotificationItem("You're in design mode :)")]);
            LaunchGameViewModel = new(null, null, null, null, null, null);
            ManageServerViewModel = new(null, null, null) { ServerName = "My fun server" };
            CreateServerViewModel = new(null) { Name = "My Server Name", SelectedGameMode = NitroxGameMode.CREATIVE };
            LibraryViewModel = new(null);
            CommunityViewModel = new(null);
            BlogViewModel = new(null, [new NitroxBlog("Design blog", DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromDays(5)), "google.com", null)]);
            UpdatesViewModel = new(null);
            OptionsViewModel = new(null, null) { SelectedGame = new() { PathToGame = @"C:\Games\Steam\steamapps\common\Subnautica", Platform = Platform.STEAM } };
            DialogBoxViewModel = new() { Title = "Title Text", Description = "Description Text" };
            ObjectPropertyEditorViewModel = new(null) { OwnerObject = new SubnauticaServerConfig() };
            BackupRestoreViewModel = new();
            ServersViewModel = new(null, null, null, null);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static MainWindowViewModel MainWindowViewModel { get; }
    public static LaunchGameViewModel LaunchGameViewModel { get; }
    public static ManageServerViewModel ManageServerViewModel { get; }
    public static CreateServerViewModel CreateServerViewModel { get; }
    public static LibraryViewModel LibraryViewModel { get; }
    public static CommunityViewModel CommunityViewModel { get; }
    public static BlogViewModel BlogViewModel { get; }
    public static UpdatesViewModel UpdatesViewModel { get; }
    public static OptionsViewModel OptionsViewModel { get; }
    public static DialogBoxViewModel DialogBoxViewModel { get; }
    public static ObjectPropertyEditorViewModel ObjectPropertyEditorViewModel { get; }
    public static BackupRestoreViewModel BackupRestoreViewModel { get; }
    public static ServersViewModel ServersViewModel { get; }
}
