using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

internal sealed partial class NotificationsViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    public partial AvaloniaList<NotificationItem> NotificationHistory { get; set; } = [];

    public NotificationsViewModel()
    {
        if (!IsDesignMode)
        {
            this.RegisterMessageListener<NotificationAddMessage, NotificationsViewModel>(static (message, vm) =>
            {
                vm.NotificationHistory.Insert(0, message.Item);
            });
        }
    }

    [RelayCommand]
    private void RemoveNotification(NotificationItem notification)
    {
        NotificationHistory.Remove(notification);
    }

    [RelayCommand]
    private void ClearAll()
    {
        NotificationHistory.Clear();
    }
}
