#if NET
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Model.Helper;

/// <summary>
///     Provides an automatically resetting asynchronous way to wait on a signal.
/// </summary>
public sealed class AsyncBarrier
{
    private TaskCompletionSource signal = new();

    public void Signal()
    {
        TaskCompletionSource tcs = Interlocked.CompareExchange(ref signal, null, null);
        tcs.TrySetResult();
    }

    public async Task WaitForSignalAsync(CancellationToken cancellationToken)
    {
        TaskCompletionSource tcs = Interlocked.CompareExchange(ref signal, null, null);
        if (tcs.Task.IsCompleted)
        {
            tcs = new TaskCompletionSource();
            Interlocked.Exchange(ref signal, tcs);
        }
        await tcs.Task.WaitAsync(cancellationToken);
    }

    public TaskAwaiter GetAwaiter()
    {
        TaskCompletionSource tcs = Interlocked.CompareExchange(ref signal, null, null);
        return tcs.Task.GetAwaiter();
    }
}
#endif
