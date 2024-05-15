using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Utils;

public static class LauncherNotifier
{
    public static void Error(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem("Error", message, NotificationType.Error)));
    }

    public static void Info(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem("Information", message)));
    }

    public static void Warning(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem("Warning", message, NotificationType.Warning)));
    }

    public static void Success(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem("Success", message, NotificationType.Success)));
    }
}
