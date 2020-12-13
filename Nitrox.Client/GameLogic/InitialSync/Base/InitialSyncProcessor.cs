using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic.InitialSync.Base
{
    public abstract class InitialSyncProcessor
    {
        public abstract IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem);

        public List<Type> DependentProcessors { get; } = new List<Type>();
    }
}
