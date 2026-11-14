using System;
using System.Collections.Generic;

namespace NitroxClient.MonoBehaviours;

internal interface IClearable
{
    void Clear();
}

public static class SessionScopedEvents
{
    private static readonly List<IClearable> all = [];

    internal static void Register(IClearable scopedEvent) => all.Add(scopedEvent);

    public static void ClearAll()
    {
        foreach (IClearable scopedEvent in all)
        {
            scopedEvent.Clear();
        }
    }
}

/// <summary>
/// An <see cref="Action"/> based event whose subscribers can be dropped as a whole via <see cref="Clear"/>.
/// </summary>
public sealed class SessionScopedEvent : IClearable
{
    private Action handlers;

    public SessionScopedEvent() => SessionScopedEvents.Register(this);

    public void Add(Action handler) => handlers += handler;
    public void Remove(Action handler) => handlers -= handler;
    public void Invoke() => handlers?.Invoke();
    public void Clear() => handlers = null;
}

/// <inheritdoc cref="SessionScopedEvent"/>
public sealed class SessionScopedEvent<T> : IClearable
{
    private Action<T> handlers;

    public SessionScopedEvent() => SessionScopedEvents.Register(this);

    public void Add(Action<T> handler) => handlers += handler;
    public void Remove(Action<T> handler) => handlers -= handler;
    public void Invoke(T arg) => handlers?.Invoke(arg);
    public void Clear() => handlers = null;
}
