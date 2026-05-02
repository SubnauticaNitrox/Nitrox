using Nitrox.Server.Subnautica.Models.AppEvents.Core;

namespace Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

internal abstract class SequentialTrigger<TEventArgs>(Func<IEvent<TEventArgs>[]> handlers) : EventTrigger<TEventArgs>(handlers)
{
    public async Task InvokeAsync(TEventArgs args)
    {
        foreach (IEvent<TEventArgs> handler in Handlers.Value)
        {
            await handler.OnEventAsync(args);
        }
    }
}
