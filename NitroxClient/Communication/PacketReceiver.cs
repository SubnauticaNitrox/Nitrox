using System.Collections.Generic;
using NitroxClient.Debuggers;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    // TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class PacketReceiver
    {
        private readonly Queue<Packet> receivedPackets;

        public PacketReceiver()
        {
            receivedPackets = new Queue<Packet>();
        }

        public void PacketReceived(Packet packet)
        {
            NetworkDebugger.Instance.PacketReceived(packet);
            lock (receivedPackets)
            {
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
