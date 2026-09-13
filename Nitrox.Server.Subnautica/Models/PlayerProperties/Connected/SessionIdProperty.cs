using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

/// <inheritdoc cref="Nitrox.Model.Core.SessionId" />
internal sealed record SessionIdProperty : IConnectedPlayerProperty<SessionId, SessionIdProperty>
{
    private const ushort OFFLINE_SESSION_ID = 0;
    private ushort sessionId;

    /// <inheritdoc cref="Nitrox.Model.Core.SessionId" />
    public SessionId Value
    {
        get => (SessionId)Interlocked.CompareExchange(ref sessionId, OFFLINE_SESSION_ID, OFFLINE_SESSION_ID);
        set => Interlocked.Exchange(ref sessionId, (ushort)value);
    }
}
