using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Sent when a save is deleted outside the Servers view (i.e. server manage view or via file explorer).
/// </summary>
public record SaveDeletedMessage(string SaveName);

public record NotificationAddMessage(NotificationItem Item);

public record NotificationCloseMessage(NotificationItem Item);

public record ViewShownMessage(RoutableViewModelBase ViewModel);
