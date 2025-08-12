using System;
using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

public class PacketReceiver
{
    private readonly Queue<Packet> receivedPackets = new(16);
    private readonly object receivedPacketsLock = new();

    public void Add(Packet packet)
    {
        lock (receivedPacketsLock)
        {
            receivedPackets.Enqueue(packet);
        }
    }

    public Packet GetNextPacket()
    {
        lock (receivedPacketsLock)
        {
            return receivedPackets.Count == 0 ? null : receivedPackets.Dequeue();
        }
    }

    /// <summary>
    ///     Applies an operation on each packet waiting to be processed and removes it from the queue.
    /// </summary>
    public void ConsumePackets<TExtra>(Action<Packet, TExtra> consumer, TExtra extraParameter)
    {
        Packet packet = GetNextPacket();
        while (packet != null)
        {
            consumer(packet, extraParameter);
            packet = GetNextPacket();
        }
    }
}
