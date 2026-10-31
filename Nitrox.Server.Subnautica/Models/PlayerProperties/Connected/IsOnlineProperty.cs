using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed class IsOnlineProperty(SessionIdProperty sessionId) : IConnectedPlayerProperty<bool, IsOnlineProperty>
{
    private const ushort OFFLINE_SESSION_ID = 0;
    private readonly SessionIdProperty sessionId = sessionId;

    public bool Value
    {
        get => sessionId.Value != (SessionId)OFFLINE_SESSION_ID;
        set
        {
            if (value)
            {
                throw new InvalidOperationException($"Player cannot be put in an online state without a {nameof(SessionId)}!");
            }
            sessionId.Value = (SessionId)OFFLINE_SESSION_ID;
        }
    }

    public IsOnlineProperty() : this(new SessionIdProperty())
    {
    }
}
