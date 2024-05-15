using Nitrox.Launcher.Models.Design;

namespace Nitrox.Launcher.Models;

/// <summary>
///     Sent when a save is deleted outside of the Servers view (i.e. server manage view or via file explorer).
/// </summary>
public record SaveDeletedMessage(string SaveName);
public record ServerEntryPropertyChangedMessage(string PropertyName);
public record NotificationAddMessage(NotificationItem Item);
public record NotificationCloseMessage(NotificationItem Item);
