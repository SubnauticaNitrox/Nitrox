using Nitrox.Server.Subnautica.Models.AppEvents.Core;

namespace Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

internal abstract class SequentialEmptyArgsTrigger<TEventArgs>(Func<IEvent<TEventArgs>[]> handlers) : EventTrigger<TEventArgs>(handlers) where TEventArgs : class, new()
{
    private static readonly TEventArgs emptyArgs = new();

    public async Task InvokeAsync()
    {
        foreach (IEvent<TEventArgs> handler in Handlers.Value)
        {
            await handler.OnEventAsync(emptyArgs);
        }
    }
}
