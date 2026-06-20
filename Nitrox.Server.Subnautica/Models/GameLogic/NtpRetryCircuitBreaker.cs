namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class NtpRetryCircuitBreaker
{
    private readonly TimeSpan closedRetryDelay;
    private readonly TimeSpan openRetryDelay;
    private readonly int failureThreshold;

    private int consecutiveFailures;

    public NtpRetryCircuitBreaker(TimeSpan closedRetryDelay, TimeSpan openRetryDelay, int failureThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(closedRetryDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(openRetryDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(failureThreshold, 1);

        this.closedRetryDelay = closedRetryDelay;
        this.openRetryDelay = openRetryDelay;
        this.failureThreshold = failureThreshold;
    }

    public bool IsOpen { get; private set; }

    public RetryDecision RegisterFailure()
    {
        if (IsOpen)
        {
            return new(openRetryDelay, BreakerOpened: false);
        }

        consecutiveFailures++;
        if (consecutiveFailures < failureThreshold)
        {
            return new(closedRetryDelay, BreakerOpened: false);
        }

        IsOpen = true;
        return new(openRetryDelay, BreakerOpened: true);
    }

    public void RegisterSuccess()
    {
        consecutiveFailures = 0;
        IsOpen = false;
    }
}

internal readonly record struct RetryDecision(TimeSpan Delay, bool BreakerOpened);
