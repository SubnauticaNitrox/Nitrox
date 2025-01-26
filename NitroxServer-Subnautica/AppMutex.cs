using System;
using System.Threading;

namespace NitroxServer_Subnautica;

public static class AppMutex
{
    private static readonly SemaphoreSlim mutexReleaseGate = new(1);
    private static readonly SemaphoreSlim callerGate = new(1);

    public static void Hold(Action onWaitingForMutex = null, CancellationToken ct = default)
    {
        Thread thread = new(o =>
        {
            bool first = true;
            Mutex mutex = new(false, typeof(AppMutex).Assembly.FullName, out bool _);
            try
            {
                try
                {
                    while (!mutex.WaitOne(100, false))
                    {
                        ct.ThrowIfCancellationRequested();
                        if (first)
                        {
                            first = false;
                            onWaitingForMutex?.Invoke();
                        }
                    }
                }
                catch (AbandonedMutexException)
                {
                    // Mutex was abandoned in another process, it will still get acquired
                }
            }
            finally
            {
                callerGate.Release();
                mutexReleaseGate.Wait(-1);
                mutex.ReleaseMutex();
            }
        });
        mutexReleaseGate.Wait(-1, ct);
        callerGate.Wait(0, ct);
        thread.Start();

        while (!callerGate.Wait(100, ct))
        {
        }
    }

    public static void Release()
    {
        mutexReleaseGate.Release();
    }
}
