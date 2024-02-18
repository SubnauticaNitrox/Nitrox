using System;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

/// <summary>
///     Suppresses the given packet type from being sent. Disables the suppression when disposed.
/// </summary>
/// <typeparam name="T">The packet type to suppress.</typeparam>
public readonly struct PacketSuppressor<T> : IDisposable where T : Packet
{
    private static bool isSuppressed;
    public static bool IsSuppressed => isSuppressed;

    private static readonly PacketSuppressor<T> instance = new();

    public static PacketSuppressor<T> Suppress()
    {
        isSuppressed = true;
        return instance;
    }

    public void Dispose()
    {
        isSuppressed = false;
    }
}

/// <inheritdoc cref="PacketSuppressor{T}"/>
/// <typeparam name="T1">First packet type to suppress.</typeparam>
/// <typeparam name="T2">Second packet type to suppress.</typeparam>
/// <typeparam name="T3">Third packet type to suppress.</typeparam>
/// <typeparam name="T4">Fourth packet type to suppress.</typeparam>
/// <typeparam name="T5">Fifth packet type to suppress.</typeparam>
public readonly struct PacketSuppressor<T1, T2, T3, T4, T5> : IDisposable
    where T1 : Packet
    where T2 : Packet
    where T3 : Packet
    where T4 : Packet
    where T5 : Packet
{
    private static readonly PacketSuppressor<T1> instance1 = new();
    private static readonly PacketSuppressor<T2> instance2 = new();
    private static readonly PacketSuppressor<T3> instance3 = new();
    private static readonly PacketSuppressor<T4> instance4 = new();
    private static readonly PacketSuppressor<T5> instance5 = new();

    public static PacketSuppressor<T1, T2, T3, T4, T5> Suppress()
    {
        PacketSuppressor<T1>.Suppress();
        PacketSuppressor<T2>.Suppress();
        PacketSuppressor<T3>.Suppress();
        PacketSuppressor<T4>.Suppress();
        PacketSuppressor<T5>.Suppress();
        return new PacketSuppressor<T1, T2, T3, T4, T5>();
    }

    public void Dispose()
    {
        instance1.Dispose();
        instance2.Dispose();
        instance3.Dispose();
        instance4.Dispose();
        instance5.Dispose();
    }
}
