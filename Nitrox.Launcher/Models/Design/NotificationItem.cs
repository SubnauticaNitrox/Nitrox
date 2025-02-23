using System.Windows.Input;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Nitrox.Launcher.Models.Design;

public partial class NotificationItem : ObservableObject
{
    public string Message { get; }
    public NotificationType Type { get; }
    public ICommand CloseCommand { get; }

    [ObservableProperty]
    private bool dismissed;

    public NotificationItem()
    {
    }

    public NotificationItem(string message, NotificationType type = NotificationType.Information, ICommand closeCommand = null)
    {
        Message = message;
        Type = type;
        CloseCommand = closeCommand ?? new RelayCommand(() => WeakReferenceMessenger.Default.Send(new NotificationCloseMessage(this)));
    }
}
