using System;
using System.Diagnostics;

namespace Nitrox.Model.Core;

/// <summary>
///     The session id (index) of a connection. The server uses 0, players will start from 1.
/// </summary>
/// <remarks>
///     It's important that, once a session id is assigned by the server, no other connection can impersonate by using the
///     same id.
///     Force a 10 minute "hands-off" time before which this session id can be reused.
/// </remarks>
[DebuggerDisplay($"{{{nameof(id)}}}")]
public readonly record struct SessionId : IComparable<SessionId>
{
    public const int DELAY_REUSE_MINUTES = 10;
    public const ushort SERVER_ID = (ushort)PeerId.SERVER_ID;

    private readonly ushort id;

    public bool IsPlayer => id != SERVER_ID;

    private SessionId(ushort id)
    {
        this.id = id;
    }

    public static explicit operator ushort(SessionId id)
    {
        return id.id;
    }

    public static explicit operator SessionId(ushort id)
    {
        return new SessionId(id);
    }

    public static bool operator <(SessionId a, SessionId b) => a.id < b.id;

    public static bool operator >(SessionId a, SessionId b) => a.id > b.id;
    public static bool operator !=(SessionId a, ushort b) => a.id != b;

    public static bool operator ==(SessionId a, ushort b) => a.id == b;

    public int CompareTo(SessionId other) => id.CompareTo(other.id);

    public override string ToString() => id.ToString();
}
