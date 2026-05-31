using System.Threading.Channels;

namespace Nitrox.Server.Subnautica.Services.Core;

/// <summary>
///     A background service that queues actions to be processed one after another, with the goal to eliminate race conditions.
/// </summary>
internal abstract class QueuingBackgroundService<TActionEntry> : BackgroundService
{
    private readonly Channel<TActionEntry> actionQueue = Channel.CreateUnbounded<TActionEntry>(new UnboundedChannelOptions { SingleReader = true });

    protected abstract Task ExecuteQueuedActionAsync(TActionEntry action, CancellationToken stoppingToken);

    public ValueTask QueueActionAsync(TActionEntry item, CancellationToken cancellationToken = default) => actionQueue.Writer.WriteAsync(item, cancellationToken);

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (TActionEntry action in actionQueue.Reader.ReadAllAsync(stoppingToken))
        {
            await ExecuteQueuedActionAsync(action, stoppingToken);
        }
    }
}
