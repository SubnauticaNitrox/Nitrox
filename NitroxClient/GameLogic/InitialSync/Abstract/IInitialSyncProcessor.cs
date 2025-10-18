using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.InitialSync.Abstract;

public interface IInitialSyncProcessor<in TPacket> where TPacket : Packet
{
    HashSet<Type> DependentProcessors { get; }

    IEnumerator Process(TPacket packet, WaitScreen.ManualWaitItem waitScreenItem);
}

public interface IInitialSyncProcessor : IInitialSyncProcessor<InitialPlayerSync>
{
}
