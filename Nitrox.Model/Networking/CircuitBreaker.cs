namespace Nitrox.Model.Networking;

/// <summary>
///     Minimal circuit breaker that counts consecutive failures and "opens" once a threshold is reached,
///     signalling callers to back off. A single success closes it again.
/// </summary>
/// <remarks>
///     Timing (how long to stay open, when to probe again) is intentionally left to the caller so this type
///     stays free of timers and trivially testable. Thread-safe so it can be shared across the retry timer
///     and network callback threads.
/// </remarks>
public sealed class CircuitBreaker(int failureThreshold)
{
    private readonly object gate = new();
    private int consecutiveFailures;

    public bool IsOpen
    {
        get
        {
            lock (gate)
            {
                return consecutiveFailures >= failureThreshold;
            }
        }
    }

    /// <summary>
    ///     Resets the failure count, closing the breaker.
    /// </summary>
    public void RecordSuccess()
    {
        lock (gate)
        {
            consecutiveFailures = 0;
        }
    }

    /// <summary>
    ///     Records a failure.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> only on the failure that opens the breaker (i.e. the threshold is reached
    ///     exactly now), so callers can react once. Subsequent failures while already open return
    ///     <see langword="false" />.
    /// </returns>
    public bool RecordFailure()
    {
        lock (gate)
        {
            if (consecutiveFailures >= failureThreshold)
            {
                return false;
            }
            consecutiveFailures++;
            return consecutiveFailures >= failureThreshold;
        }
    }
}
