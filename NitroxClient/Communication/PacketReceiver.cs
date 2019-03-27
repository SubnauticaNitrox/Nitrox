using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Util;

namespace NitroxClient.Communication
{
    // TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class PacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;

        private readonly NitroxModel.DataStructures.PriorityQueue<Packet> receivedPackets;
        private readonly VisibleCells visibleCells;

        public PacketReceiver(VisibleCells visibleCells)
        {
            this.visibleCells = visibleCells;
            receivedPackets = new NitroxModel.DataStructures.PriorityQueue<Packet>();
        }

        public void PacketReceived(Packet packet)
        {
            lock (receivedPackets)
            {
                receivedPackets.Enqueue(DEFAULT_PACKET_PRIORITY, packet);
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
