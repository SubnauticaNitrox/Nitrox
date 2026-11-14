using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Administration.Core;

namespace Nitrox.Server.Subnautica.Models.Administration;

internal interface ITeleport : IAdminFeature<ITeleport>
{
    /// <summary>
    ///     Teleports a player to the given location and subroot.
    /// </summary>
    /// <param name="sessionId">The session id of a player.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="subRootId">The destination subroot.</param>
    Task TeleportAsync(SessionId sessionId, NitroxVector3 destination, Optional<NitroxId> subRootId);
}
