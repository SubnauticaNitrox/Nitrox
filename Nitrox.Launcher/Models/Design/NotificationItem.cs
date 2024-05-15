using System.Windows.Input;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Design;

public record NotificationItem
{
    public string Title { get; }
    public string Description { get; }
    public NotificationType Type { get; }
    public ICommand CloseCommand { get; }

    public NotificationItem(string title, string description, NotificationType type = NotificationType.Information, ICommand closeCommand = null)
    {
        Title = title;
        Description = description;
        Type = type;
        CloseCommand = closeCommand ?? ReactiveCommand.Create(() => WeakReferenceMessenger.Default.Send(new NotificationCloseMessage(this)));
    }
}
