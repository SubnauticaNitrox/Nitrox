using System;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

/// <summary>
///     Suppresses the given packet type from being sent. Disables the suppression when disposed.
/// </summary>
/// <typeparam name="T">The packet type to suppress.</typeparam>
public readonly struct PacketSuppressor<T> : IDisposable
    where T : Packet
{
    private static bool isSuppressed;
    public static bool IsSuppressed => isSuppressed;

    public static readonly PacketSuppressor<T> Instance = new();

    public static PacketSuppressor<T> Suppress()
    {
        isSuppressed = true;
        return Instance;
    }

    public void Dispose()
    {
        isSuppressed = false;
    }
}
