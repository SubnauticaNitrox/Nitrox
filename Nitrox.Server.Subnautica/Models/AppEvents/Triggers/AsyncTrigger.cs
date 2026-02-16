using System.Buffers;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;

namespace Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

internal abstract class AsyncTrigger<TEventArgs>(Func<IEvent<TEventArgs>[]> handlers) : EventTrigger<TEventArgs>(handlers)
{
    private static readonly ArrayPool<Task> pool = ArrayPool<Task>.Create();

    public async Task InvokeAsync(TEventArgs args)
    {
        IEvent<TEventArgs>[] handlers = Handlers.Value;
        Task[] tasks = pool.Rent(handlers.Length);
        try
        {
            for (int i = 0; i < handlers.Length; i++)
            {
                tasks[i] = handlers[i].OnEventAsync(args);
            }
            await Task.WhenAll(tasks.AsSpan(0, handlers.Length));
        }
        finally
        {
            pool.Return(tasks);
        }
    }
}
