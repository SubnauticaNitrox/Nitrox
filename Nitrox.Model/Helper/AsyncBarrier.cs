#if NET9_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Model.Helper;

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

    public async Task WaitForSignalAsync(CancellationToken cancellationToken)
    {
        await AtomicRefreshTcs(ref signal).WaitAsync(cancellationToken);
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
                tcs = new();
                return tcs.Task;
            }
        }
        return tcsTask;
    }
}
#endif
