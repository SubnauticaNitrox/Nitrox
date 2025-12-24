using System;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Sent when a save is deleted outside the Servers view (i.e. server manage view or via file explorer).
/// </summary>
internal record SaveDeletedMessage(string SaveName);

internal record NotificationAddMessage(NotificationItem Item);

internal record NotificationCloseMessage(NotificationItem Item);

internal record ShowViewMessage
{
    public required RoutableViewModelBase ViewModel { get; init; }
}

internal record ShowPreviousViewMessage(Type? RoutableViewModelType = null);

internal record ServerStatusMessage(int ProcessId, bool IsOnline, int PlayerCount = 0);
