using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    // TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class DeferringPacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;

        private readonly Dictionary<AbsoluteEntityCell, Queue<Packet>> deferredPacketsByAbsoluteCell = new Dictionary<AbsoluteEntityCell, Queue<Packet>>();
        private NitroxModel.DataStructures.PriorityQueue<Packet> receivedPackets;
        private readonly VisibleCells visibleCells;

        public DeferringPacketReceiver(VisibleCells visibleCells)
        {
            this.visibleCells = visibleCells;
            receivedPackets = new NitroxModel.DataStructures.PriorityQueue<Packet>();
        }

        public void PacketReceived(Packet packet)
        {
            lock (receivedPackets)
            {
                if (!PacketWasDeferred(packet))
                {
                    receivedPackets.Enqueue(DEFAULT_PACKET_PRIORITY, packet);
                }
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

        private bool PacketWasDeferred(Packet packet)
        {
            if (packet is RangedPacket)
            {
                RangedPacket playerAction = (RangedPacket)packet;

                if (visibleCells.Contains(playerAction.AbsoluteEntityCell))
                {
                    return false;
                }

                Log.Debug($"Action {packet} was deferred, cell not loaded (with required lod): {playerAction.AbsoluteEntityCell}");
                AddPacketToDeferredMap(playerAction, playerAction.AbsoluteEntityCell);
                return true;
            }

            return false;
        }

        private void AddPacketToDeferredMap(RangedPacket rangedPacket, AbsoluteEntityCell cell)
        {
            lock (deferredPacketsByAbsoluteCell)
            {
                if (!deferredPacketsByAbsoluteCell.ContainsKey(cell))
                {
                    deferredPacketsByAbsoluteCell.Add(cell, new Queue<Packet>());
                }

                deferredPacketsByAbsoluteCell[cell].Enqueue(rangedPacket);
            }
        }

        public void CellLoaded(AbsoluteEntityCell absoluteEntityCell)
        {
            lock (deferredPacketsByAbsoluteCell)
            {
                Queue<Packet> deferredPackets;
                if (deferredPacketsByAbsoluteCell.TryGetValue(absoluteEntityCell, out deferredPackets))
                {
                    Log.Debug("Loaded {0}; found {1} deferred packet(s):{2}\nAdding it back with high priority.",
                        absoluteEntityCell,
                        deferredPackets.Count,
                        deferredPackets.PrefixWith("\n\t"));
                    while (deferredPackets.Count > 0)
                    {
                        Packet packet = deferredPackets.Dequeue();
                        receivedPackets.Enqueue(EXPIDITED_PACKET_PRIORITY, packet);
                    }
                }
            }
        }

        public void Flush()
        {
            lock (receivedPackets)
            {
                receivedPackets = new NitroxModel.DataStructures.PriorityQueue<Packet>();
                deferredPacketsByAbsoluteCell.Clear();
            }
        }
    }
}
