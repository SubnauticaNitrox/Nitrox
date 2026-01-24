using Nitrox.Server.Subnautica.Models.AppEvents.Core;

namespace Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

internal abstract class AsyncTrigger<TEventArgs>(Func<IEvent<TEventArgs>[]> handlers) : EventTrigger<TEventArgs>(handlers)
{
    public async Task InvokeAsync(TEventArgs args)
    {
        IEvent<TEventArgs>[] handlers = Handlers.Value;
        Task[] tasks = new Task[handlers.Length];
        for (int i = 0; i < handlers.Length; i++)
        {
            tasks[i] = handlers[i].OnEventAsync(args);
        }
        await Task.WhenAll(tasks);
    }
}
