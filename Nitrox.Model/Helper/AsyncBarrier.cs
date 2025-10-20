#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Model.Helper;

/// <summary>
///     Provides an automatically resetting asynchronous way to wait on a signal.
/// </summary>
public sealed class AsyncBarrier
{
    private readonly LockObject locker = new();
    private TaskCompletionSource signal = new();

    public void Signal()
    {
        lock (locker)
        {
            signal.TrySetResult();
        }
    }

    public async Task WaitForSignalAsync(CancellationToken cancellationToken) => await AtomicRefreshTcs(ref signal).WaitAsync(cancellationToken);

    public TaskAwaiter GetAwaiter()
    {
        lock (locker)
        {
            return signal.Task.GetAwaiter();
        }
    }

    private Task AtomicRefreshTcs(ref TaskCompletionSource tcs)
    {
        Task tcsTask;
        lock (locker)
        {
            tcsTask = tcs.Task;
        }
        if (tcsTask.IsCompletedSuccessfully)
        {
            lock (locker)
            {
                tcs = new TaskCompletionSource();
                return tcs.Task;
            }
        }
        return tcsTask;
    }
}
#endif
