using System.Collections.Generic;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

internal interface IPlayerPropProvider
{
    /// <summary>
    ///     Gets all the properties of the given type from all connected players.
    /// </summary>
    public IEnumerable<(SessionId SessionId, T Property)> GetAll<T>() where T : class, IPlayerProperty;

    /// <summary>
    ///     Gets a player property for the peer id, if the player exists.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <see cref="peerId" /> is less than 1.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If no session data is available for the given peer id.
    /// </exception>
    public T Get<T>(PeerId peerId) where T : class, IPlayerProperty;

    /// <summary>
    ///     Gets a player property for the session id, if the player exists.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <see cref="sessionId" /> is less than 1.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If no session data is available for the given session id.
    /// </exception>
    public T Get<T>(SessionId sessionId) where T : class, IPlayerProperty;
}
