using System.Collections.Concurrent;
using System.Diagnostics;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed class ScannerRoomQueryLimiter
{
    private readonly ConcurrentDictionary<SessionId, QueryState> states = new();
    private readonly long minimumIntervalTicks;

    public ScannerRoomQueryLimiter() : this(TimeSpan.FromMilliseconds(500))
    {
    }

    internal ScannerRoomQueryLimiter(TimeSpan minimumInterval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumInterval, TimeSpan.Zero);
        minimumIntervalTicks = checked((long)Math.Ceiling(minimumInterval.TotalSeconds * Stopwatch.Frequency));
    }

    public async Task<IDisposable> EnterAsync(SessionId sessionId)
    {
        QueryState state = states.GetOrAdd(sessionId, _ => new QueryState());
        await state.Gate.WaitAsync();
        try
        {
            long now = Stopwatch.GetTimestamp();
            long previous = Interlocked.Read(ref state.LastStarted);
            long remainingTicks = minimumIntervalTicks - (now - previous);
            if (previous != 0 && remainingTicks > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(remainingTicks / (double)Stopwatch.Frequency));
            }

            Interlocked.Exchange(ref state.LastStarted, Stopwatch.GetTimestamp());
            return new QueryLease(state.Gate);
        }
        catch
        {
            state.Gate.Release();
            throw;
        }
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
