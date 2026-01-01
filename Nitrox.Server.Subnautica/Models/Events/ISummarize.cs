using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Events;

/// <summary>
///     Implementations provide a user-friendly summary of their state.
/// </summary>
internal interface ISummarize
{
    /// <summary>
    ///     Issues a summary of interest to the end user from the state of the current type.
    ///     The implementation should use an <see cref="ILogger" /> for output.
    /// </summary>
    Task LogSummaryAsync(Perms viewerPerms);
}
