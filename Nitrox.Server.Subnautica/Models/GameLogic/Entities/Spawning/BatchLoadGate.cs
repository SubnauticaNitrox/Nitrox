namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

/// <summary>
/// Serializes cold batch parsing and spawning while those operations depend on shared mutable state.
/// </summary>
internal sealed class BatchLoadGate
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public async Task<T> RunAsync<T>(Func<Task<T>> operation)
    {
        await semaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
