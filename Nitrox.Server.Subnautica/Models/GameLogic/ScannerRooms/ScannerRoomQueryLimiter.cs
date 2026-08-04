using System.Collections.Concurrent;
using System.Diagnostics;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed class ScannerRoomQueryLimiter
{
    private static readonly long minimumIntervalTicks = Stopwatch.Frequency / 2;
    private readonly ConcurrentDictionary<SessionId, QueryState> states = new();

    public bool TryEnter(SessionId sessionId, out IDisposable? lease)
    {
        QueryState state = states.GetOrAdd(sessionId, _ => new QueryState());
        if (!state.Gate.Wait(0))
        {
            lease = null;
            return false;
        }

        long now = Stopwatch.GetTimestamp();
        long previous = Interlocked.Read(ref state.LastStarted);
        if (previous != 0 && now - previous < minimumIntervalTicks)
        {
            state.Gate.Release();
            lease = null;
            return false;
        }

        Interlocked.Exchange(ref state.LastStarted, now);
        lease = new QueryLease(state.Gate);
        return true;
    }

    private sealed class QueryState
    {
        public readonly SemaphoreSlim Gate = new(1, 1);
        public long LastStarted;
    }

    private sealed class QueryLease(SemaphoreSlim gate) : IDisposable
    {
        private SemaphoreSlim? gate = gate;

        public void Dispose() => Interlocked.Exchange(ref gate, null)?.Release();
    }
}
