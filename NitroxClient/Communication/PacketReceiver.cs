using System.Collections.Generic;
using NitroxClient.Debuggers;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public class PacketReceiver
    {
        private readonly INetworkDebugger networkDebugger;
        private readonly Queue<Packet> receivedPackets;

        public PacketReceiver(INetworkDebugger networkDebugger = null)
        {
            receivedPackets = new Queue<Packet>();
            this.networkDebugger = networkDebugger;
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
            Queue<Packet> packets = new();
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
