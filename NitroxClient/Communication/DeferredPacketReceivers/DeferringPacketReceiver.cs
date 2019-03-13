using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Util;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.DeferredPacketReceivers
{
    // TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class DeferringPacketReceiver : DeferredPacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;

        private readonly Dictionary<AbsoluteEntityCell, Queue<Packet>> deferredPacketsByAbsoluteCell = new Dictionary<AbsoluteEntityCell, Queue<Packet>>();
        private readonly NitroxModel.DataStructures.PriorityQueue<Packet> receivedPackets;
        private readonly VisibleCells visibleCells;

        public DeferringPacketReceiver(VisibleCells visibleCells)
        {
            this.visibleCells = visibleCells;
            receivedPackets = new NitroxModel.DataStructures.PriorityQueue<Packet>();
        }

        public override void PacketReceived(Packet packet)
        {
            lock (receivedPackets)
            {
                if (!PacketWasDeferred(packet))
                {
                    receivedPackets.Enqueue(DEFAULT_PACKET_PRIORITY, packet);
                }
            }
        }

        public override Queue<Packet> GetReceivedPackets()
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

        internal override bool PacketWasDeferred(Packet packet)
        {
            Optional<AbsoluteEntityCell> deferLocation = packet.GetDeferredCell();

            if (deferLocation.IsPresent())
            {
                AbsoluteEntityCell mustBeLoadedCell = deferLocation.Get();

                if (visibleCells.Contains(mustBeLoadedCell))
                {
                    return false;
                }

                Log.Debug($"Packet {packet} was deferred, cell not loaded (with required lod): {mustBeLoadedCell}");
                AddPacketToDeferredMap(packet, mustBeLoadedCell);
                return true;
            }

            return false;
        }

        internal override void AddPacketToDeferredMap(Packet deferred, AbsoluteEntityCell cell)
        {
            lock (deferredPacketsByAbsoluteCell)
            {
                Queue<Packet> queue;
                if (!deferredPacketsByAbsoluteCell.TryGetValue(cell, out queue))
                {
                    deferredPacketsByAbsoluteCell[cell] = queue = new Queue<Packet>();
                }

                queue.Enqueue(deferred);
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
    }
}
