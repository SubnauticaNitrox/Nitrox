using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

// TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
public class PacketReceiver
{
    private readonly object receivedPacketsLock = new();
    private readonly Queue<Packet> receivedPackets = new();

    public void PacketReceived(Packet packet)
    {
        lock (receivedPacketsLock)
        {
            receivedPackets.Enqueue(packet);
        }
    }

    public Queue<Packet> GetReceivedPackets()
    {
        Queue<Packet> packets = new();

        lock (receivedPacketsLock)
        {
            while (receivedPackets.Count > 0)
            {
                packets.Enqueue(receivedPackets.Dequeue());
            }
        }

        return packets;
    }
}
