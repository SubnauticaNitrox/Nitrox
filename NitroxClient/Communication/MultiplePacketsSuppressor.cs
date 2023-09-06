using System;
using System.Collections.Generic;

namespace NitroxClient.Communication;

/// <summary>
///     Suppresses the given packet types from being sent. Disables the suppression when disposed.
/// </summary>
public readonly struct MultiplePacketsSuppressor : IDisposable
{
    private static readonly List<Type> globalPacketSuppressed = new();

    public static MultiplePacketsSuppressor Suppress(Type[] typesToSuppress) => new(typesToSuppress);

    public static bool IsSuppressed(Type type) => globalPacketSuppressed.Contains(type);


    private readonly Type[] packetSuppressed;

    public MultiplePacketsSuppressor(Type[] typesToSuppress)
    {
        packetSuppressed = typesToSuppress;

        foreach (Type type in typesToSuppress)
        {
            globalPacketSuppressed.Add(type);
        }
    }

    public void Dispose()
    {
        foreach (Type type in packetSuppressed)
        {
            globalPacketSuppressed.Remove(type);
        }
    }
}
