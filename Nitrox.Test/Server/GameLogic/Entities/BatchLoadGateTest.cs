using System.Threading.Tasks;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

namespace Nitrox.Test.Server.GameLogic.Entities;

[TestClass]
public sealed class BatchLoadGateTest
{
    [TestMethod]
    public async Task DifferentColdLoadsNeverRunConcurrently()
    {
        BatchLoadGate gate = new();
        TaskCompletionSource firstEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirst = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        object counterLock = new();
        int activeLoads = 0;
        int maximumConcurrentLoads = 0;

        Task<int> firstLoad = gate.RunAsync(async () =>
        {
            RecordEntry();
            firstEntered.SetResult();
            await releaseFirst.Task;
            RecordExit();
            return 1;
        });
        await firstEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Task<int> secondLoad = gate.RunAsync(async () =>
        {
            RecordEntry();
            secondEntered.SetResult();
            await Task.Yield();
            RecordExit();
            return 2;
        });

        bool secondStartedBeforeFirstCompleted = secondEntered.Task.IsCompleted;
        releaseFirst.SetResult();
        int[] results = await Task.WhenAll(firstLoad, secondLoad).WaitAsync(TimeSpan.FromSeconds(5));

        secondStartedBeforeFirstCompleted.Should().BeFalse();
        maximumConcurrentLoads.Should().Be(1);
        results.Should().Equal(1, 2);

        void RecordEntry()
        {
            lock (counterLock)
            {
                activeLoads++;
                maximumConcurrentLoads = Math.Max(maximumConcurrentLoads, activeLoads);
            }
        }

        void RecordExit()
        {
            lock (counterLock)
            {
                activeLoads--;
            }
        }
    }

    [TestMethod]
    public async Task FailedColdLoadReleasesGate()
    {
        BatchLoadGate gate = new();
        Func<Task> failingLoad = () => gate.RunAsync(() => Task.FromException<int>(new InvalidOperationException("parse failed")));

        await failingLoad.Should().ThrowAsync<InvalidOperationException>();

        int result = await gate.RunAsync(() => Task.FromResult(42)).WaitAsync(TimeSpan.FromSeconds(5));
        result.Should().Be(42);
    }
}
