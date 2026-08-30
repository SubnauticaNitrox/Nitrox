namespace NitroxClient.GameLogic.PictureFrames
{
    [TestClass]
    public class SessionByteBudgetTests
    {
        [TestMethod]
        public void ShouldHaveBudgetWhenNothingConsumed()
        {
            SessionByteBudget budget = new(() => 100);

            budget.HasBudget.Should().BeTrue();
            budget.ConsumedBytes.Should().Be(0);
        }

        [TestMethod]
        public void ShouldReduceBudgetAsBytesAreConsumed()
        {
            SessionByteBudget budget = new(() => 100);

            budget.Consume(40);

            budget.ConsumedBytes.Should().Be(40);
            budget.HasBudget.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldLoseBudgetOnceCapIsReached()
        {
            SessionByteBudget budget = new(() => 100);

            budget.Consume(60);
            budget.Consume(40);

            budget.HasBudget.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldOnlyReportCapReachedOnce()
        {
            SessionByteBudget budget = new(() => 100);
            budget.Consume(150);

            budget.TryMarkCapReachedOnce().Should().BeTrue();
            budget.TryMarkCapReachedOnce().Should().BeFalse();
            budget.TryMarkCapReachedOnce().Should().BeFalse();
        }

        [TestMethod]
        public void ShouldNotReportCapReachedBeforeItIsHit()
        {
            SessionByteBudget budget = new(() => 100);
            budget.Consume(50);

            budget.TryMarkCapReachedOnce().Should().BeFalse();
        }

        [TestMethod]
        public void ShouldReflectLiveChangesToTheCapProvider()
        {
            long cap = 100;
            SessionByteBudget budget = new(() => cap);
            budget.Consume(80);

            budget.HasBudget.Should().BeTrue();

            cap = 50;

            budget.HasBudget.Should().BeFalse();
        }

        [TestMethod]
        public void MbToBytes_SubOneMegabyte_DoesNotTruncateToZero()
        {
            SessionByteBudget.MbToBytes(0.256f).Should().Be(268435);
        }

        [TestMethod]
        public void MbToBytes_WholeMegabytes_ConvertsExactly()
        {
            SessionByteBudget.MbToBytes(256f).Should().Be(256L * 1024 * 1024);
        }
    }
}
