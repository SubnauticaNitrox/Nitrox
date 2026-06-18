using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Test.Server.GameLogic;

[TestClass]
public class NtpRetryCircuitBreakerTests
{
    private static readonly TimeSpan SHORT_DELAY = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan LONG_DELAY = TimeSpan.FromMinutes(10);

    [TestMethod]
    public void RegisterFailure_UsesShortDelayUntilThreshold()
    {
        NtpRetryCircuitBreaker breaker = new(SHORT_DELAY, LONG_DELAY, failureThreshold: 3);

        RetryDecision firstDecision = breaker.RegisterFailure();
        RetryDecision secondDecision = breaker.RegisterFailure();

        firstDecision.Delay.Should().Be(SHORT_DELAY);
        secondDecision.Delay.Should().Be(SHORT_DELAY);
        firstDecision.BreakerOpened.Should().BeFalse();
        secondDecision.BreakerOpened.Should().BeFalse();
        breaker.IsOpen.Should().BeFalse();
    }

    [TestMethod]
    public void RegisterFailure_OpensBreakerAtThreshold()
    {
        NtpRetryCircuitBreaker breaker = new(SHORT_DELAY, LONG_DELAY, failureThreshold: 3);

        breaker.RegisterFailure();
        breaker.RegisterFailure();
        RetryDecision decision = breaker.RegisterFailure();

        decision.Delay.Should().Be(LONG_DELAY);
        decision.BreakerOpened.Should().BeTrue();
        breaker.IsOpen.Should().BeTrue();
    }

    [TestMethod]
    public void RegisterFailure_KeepsUsingLongDelayWhileOpen()
    {
        NtpRetryCircuitBreaker breaker = new(SHORT_DELAY, LONG_DELAY, failureThreshold: 1);

        breaker.RegisterFailure();
        RetryDecision decision = breaker.RegisterFailure();

        decision.Delay.Should().Be(LONG_DELAY);
        decision.BreakerOpened.Should().BeFalse();
        breaker.IsOpen.Should().BeTrue();
    }

    [TestMethod]
    public void RegisterSuccess_ClosesBreakerAndResetsFailureCount()
    {
        NtpRetryCircuitBreaker breaker = new(SHORT_DELAY, LONG_DELAY, failureThreshold: 2);

        breaker.RegisterFailure();
        breaker.RegisterFailure();
        breaker.RegisterSuccess();
        RetryDecision decision = breaker.RegisterFailure();

        breaker.IsOpen.Should().BeFalse();
        decision.Delay.Should().Be(SHORT_DELAY);
        decision.BreakerOpened.Should().BeFalse();
    }
}
