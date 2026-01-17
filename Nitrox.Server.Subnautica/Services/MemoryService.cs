namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Without bugs in code, this service isn't needed.
///     However, we use AssetsTools.NET which holds open file handlers without a way to close them (without reflection).
///     That's where this <see cref="MemoryService" /> comes in, to force the GC to check if any file handler (pointers)
///     are reachable and if not, deallocate them.
/// </summary>
internal sealed class MemoryService(ILogger<MemoryService> logger) : BackgroundService
{
    private readonly ILogger<MemoryService> logger = logger;
    private readonly AsyncBarrier memCompactBarrier = new();

    /// <summary>
    ///     Queues a memory compaction loop to be executed as soon as possible.
    /// </summary>
    /// <remarks>
    ///     This forces the GC to check on dangling memory in case of bad memory management. Ideally, this method should not exist.
    /// </remarks>
    public void QueueCompact()
    {
        memCompactBarrier.Signal();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await memCompactBarrier.WaitForSignalAsync(stoppingToken);
            await CompactMemoryAsync(stoppingToken);
        }
    }

    private async Task CompactMemoryAsync(CancellationToken cancellationToken)
    {
        const long MIN_EXPECTED_MEM_CHANGE = 1024 * 1024 * 10; // 10 MiB
        const int STARTING_RETRIES = 3;
        const int MAX_ITERATIONS = 50;

        long curMem = Environment.WorkingSet;
        int retries = STARTING_RETRIES;
        int iterations = 0;
        while (true)
        {
            long prevMem = curMem;
            GC.Collect();
            await Task.Delay(500 + Random.Shared.Next(-100, 100), cancellationToken);
            curMem = Environment.WorkingSet;
            long memChange = long.Clamp(prevMem - curMem, 0, long.MaxValue);
            logger.ZLogTrace($"Freed {((uint)memChange).AsByteUnitText()}");
            if (++iterations >= MAX_ITERATIONS)
            {
                break;
            }
            if (memChange >= MIN_EXPECTED_MEM_CHANGE)
            {
                retries = STARTING_RETRIES;
            }
            if (--retries < 0)
            {
                break;
            }
        }
        logger.ZLogTrace($"Stopped compacting memory");
    }
}
