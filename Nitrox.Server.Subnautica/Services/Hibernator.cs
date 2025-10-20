using System.Collections.Concurrent;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class Hibernator
{
    private readonly ConcurrentBag<Action> sleepTasks = [];
    private readonly ConcurrentBag<Action> wakeTasks = [];
    private bool isSleeping;
    public bool IsSleeping => Interlocked.CompareExchange(ref isSleeping, true, true);

    public void AddSleepTask(Action action)
    {
        sleepTasks.Add(action);
    }

    public void AddWakeTask(Action action)
    {
        wakeTasks.Add(action);
    }
    
    public void Sleep()
    {
        Interlocked.Exchange(ref isSleeping, true);
        foreach (Action task in sleepTasks)
        {
            task();
        }
    }

    public void Wake()
    {
        Interlocked.Exchange(ref isSleeping, false);
        foreach (Action task in wakeTasks)
        {
            task();
        }
    }
}