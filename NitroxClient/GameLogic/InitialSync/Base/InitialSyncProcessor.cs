using System;
using System.Collections;
using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync.Base;

public abstract class InitialSyncProcessor
{
    public virtual List<IEnumerator> GetSteps(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem) => new();

    public virtual bool AutomaticProgress => false;

    public virtual IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        List<IEnumerator> steps = GetSteps(packet, waitScreenItem);
        for (int i = 0; i < steps.Count; i++)
        {
            yield return steps[i];
            if (AutomaticProgress)
            {
                waitScreenItem.SetProgress(i / steps.Count);
            }
            yield return null;
        }
    }

    public List<Type> DependentProcessors { get; } = new();
}
