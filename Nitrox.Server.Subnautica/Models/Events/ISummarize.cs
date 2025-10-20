using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Events;

/// <summary>
///     Implementors provide a user-friendly summary of their state.
/// </summary>
internal interface ISummarize
{
    /// <summary>
    ///     Gets a summary of interest to the end user from the state of the current type.
    /// </summary>
    IAsyncEnumerable<string> GetSummaryAsync(Perms viewerPerms);
}
