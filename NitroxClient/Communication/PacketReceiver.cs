using System.Collections.Generic;
using NitroxClient.Debuggers;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    // TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class PacketReceiver
    {
        private readonly NetworkDebugger networkDebugger;
        private readonly Queue<Packet> receivedPackets;

        public PacketReceiver()
        {
            receivedPackets = new Queue<Packet>();

            if (NitroxEnvironment.IsNormal) //Testing would fail because NetworkDebugger inherits from Unity.
            {
                networkDebugger = NitroxServiceLocator.LocateService<NetworkDebugger>();
            }
        }

        public void PacketReceived(Packet packet)
        {
            lock (receivedPackets)
            {
                networkDebugger?.PacketReceived(packet);
                receivedPackets.Enqueue(packet);
            }
        }

        public Queue<Packet> GetReceivedPackets()
        {
            Queue<Packet> packets = new Queue<Packet>();

            lock (receivedPackets)
            {
                while (receivedPackets.Count > 0)
                {
                    packets.Enqueue(receivedPackets.Dequeue());
                }
            }

            return packets;
        }
    }
}
