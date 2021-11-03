using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using ToastNotifications.Core;

namespace NitroxLauncher
{
    internal static class LauncherNotifier
    {
        private static readonly Notifier notifier = new(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 10);

            cfg.DisplayOptions.TopMost = false;

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(20),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public static void Info(string message, MessageOptions options = null) => notifier.ShowInformation(message, options ?? new MessageOptions());

        public static void Error(string message, MessageOptions options = null) => notifier.ShowError(message, options ?? new MessageOptions());

        public static void Success(string message, MessageOptions options = null) => notifier.ShowSuccess(message, options ?? new MessageOptions());

        public static void Warning(string message, MessageOptions options = null) => notifier.ShowWarning(message, options ?? new MessageOptions());
    }
}
