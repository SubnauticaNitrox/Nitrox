using Nitrox.Server.Subnautica.Database.Models;

namespace Nitrox.Server.Subnautica.Models.Respositories.Core;

/// <summary>
///     Implementors migrate session data away from the disconnected session.
/// </summary>
public interface ISessionCleaner
{
    /// <summary>
    ///     Gets the requested priority of the session cleaner. Higher values are called before lower values.
    /// </summary>
    public int SessionCleanPriority => 0;

    Task CleanSessionAsync(PlayerSession disconnectedSession);
}
