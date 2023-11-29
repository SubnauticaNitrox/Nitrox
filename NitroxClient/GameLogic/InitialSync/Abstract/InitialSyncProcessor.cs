using System;
using System.Collections;
using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync.Abstract;

public abstract class InitialSyncProcessor : IInitialSyncProcessor
{
    public virtual List<Func<InitialPlayerSync, IEnumerator>> Steps { get; } = new();
    public virtual HashSet<Type> DependentProcessors { get; } = new();

    public virtual IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            yield return Steps[i](packet);
            waitScreenItem.SetProgress((float)i / Steps.Count);
            yield return null;
        }
    }

    public void AddDependency<TDependency>() where TDependency : IInitialSyncProcessor
    {
        DependentProcessors.Add(typeof(TDependency));
    }

    public void AddStep(Func<InitialPlayerSync, IEnumerator> step)
    {
        Steps.Add(step);
    }

    public void AddStep(Action<InitialPlayerSync> step)
    {
        Steps.Add(sync =>
        {
            step(sync);
            return Array.Empty<object>().GetEnumerator();
        });
    }

    public void AddStep(Func<IEnumerator> step)
    {
        Steps.Add(_ => step());
    }
}
