using System;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// An <see cref="Action"/> based event whose subscribers can be dropped as a whole via <see cref="Clear"/>,
/// so the owner can ensure none linger across multiplayer sessions
/// </summary>
public sealed class SessionScopedEvent
{
    private Action handlers;

    public void Add(Action handler) => handlers += handler;
    public void Remove(Action handler) => handlers -= handler;
    public void Invoke() => handlers?.Invoke();
    public void Clear() => handlers = null;
}

/// <inheritdoc cref="SessionScopedEvent"/>
public sealed class SessionScopedEvent<T>
{
    private Action<T> handlers;

    public void Add(Action<T> handler) => handlers += handler;
    public void Remove(Action<T> handler) => handlers -= handler;
    public void Invoke(T arg) => handlers?.Invoke(arg);
    public void Clear() => handlers = null;
}
