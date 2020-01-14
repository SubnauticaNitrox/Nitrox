using System;
using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync.Base
{
    public abstract class InitialSyncProcessor
    {
        public abstract void Process(InitialPlayerSync packet);

        public List<Type> DependentProcessors { get; } = new List<Type>();
    }
}
