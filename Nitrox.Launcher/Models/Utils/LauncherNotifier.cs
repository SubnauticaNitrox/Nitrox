using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models.Utils;

public static class LauncherNotifier
{
    public static void Error(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem(message, NotificationType.Error)));
    }

    public static void Info(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem(message)));
    }

    public static void Warning(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem(message, NotificationType.Warning)));
    }

    public static void Success(string message)
    {
        WeakReferenceMessenger.Default.Send(new NotificationAddMessage(new NotificationItem(message, NotificationType.Success)));
    }
}
