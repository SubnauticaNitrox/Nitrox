namespace Nitrox.Model.Networking;

[TestClass]
public class CircuitBreakerTest
{
    [TestMethod]
    public void OpensExactlyWhenThresholdIsReached()
    {
        CircuitBreaker breaker = new(3);

        breaker.IsOpen.Should().BeFalse();
        breaker.RecordFailure().Should().BeFalse(); // 1
        breaker.RecordFailure().Should().BeFalse(); // 2
        breaker.IsOpen.Should().BeFalse();
        breaker.RecordFailure().Should().BeTrue();  // 3 -> opens
        breaker.IsOpen.Should().BeTrue();
    }

    [TestMethod]
    public void ReportsTheOpeningTransitionOnlyOnce()
    {
        CircuitBreaker breaker = new(1);

        breaker.RecordFailure().Should().BeTrue();  // opens
        breaker.RecordFailure().Should().BeFalse(); // already open, no second transition
        breaker.RecordFailure().Should().BeFalse();
        breaker.IsOpen.Should().BeTrue();
    }

    [TestMethod]
    public void SuccessClosesTheBreakerAndResetsTheCount()
    {
        CircuitBreaker breaker = new(2);

        breaker.RecordFailure().Should().BeFalse(); // 1
        breaker.RecordSuccess();                    // back to 0
        breaker.RecordFailure().Should().BeFalse(); // 1 again, still closed
        breaker.IsOpen.Should().BeFalse();
        breaker.RecordFailure().Should().BeTrue();  // 2 -> opens
        breaker.IsOpen.Should().BeTrue();
    }
}
