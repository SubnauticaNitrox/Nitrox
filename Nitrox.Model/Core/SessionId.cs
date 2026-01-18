namespace Nitrox.Model.Core;

/// <summary>
///     The session id (index) of a connection. The server uses 0, players will be start from 1.
/// </summary>
/// <remarks>
///     It's important that, once a session id is assigned by the server, no other connection can impersonate by using the
///     same id.
///     Force a 10 minute "hands-off" time before which this session id can be reused.
/// </remarks>
public readonly record struct SessionId
{
    public const int DELAY_REUSE_MINUTES = 10;
    public const ushort SERVER_ID = (ushort)PeerId.SERVER_ID;

    private readonly ushort id;

    private SessionId(ushort id)
    {
        this.id = id;
    }

    public static implicit operator ushort(SessionId id)
    {
        return id.id;
    }

    public static implicit operator SessionId(ushort id)
    {
        return new SessionId(id);
    }

    public override string ToString() => id.ToString();
}
