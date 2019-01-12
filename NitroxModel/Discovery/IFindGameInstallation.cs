using System.Collections.Generic;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Discovery
{
    /// <summary>
    ///     Implementors of this interface try to find the Subnautica installation directory.
    /// </summary>
    public interface IFindGameInstallation
    {
        /// <summary>
        ///     Searches for Subnautica installation directory.
        /// </summary>
        /// <param name="errors">Error messages that can be set if it failed to find the game.</param>
        /// <returns>Optional with path to Subnautica installation directory or <see cref="Optional{T}.Empty" /> if not found.</returns>
        Optional<string> FindGame(List<string> errors);
    }
}
