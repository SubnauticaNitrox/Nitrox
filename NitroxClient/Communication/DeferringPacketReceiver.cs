using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    //TODO: Spinlocks don't seem to be necessary here, but I don't know for certain.
    public class DeferringPacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;
        private const int DESIRED_CELL_MIN_LOD_FOR_ACTIONS = 1;

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
            if (packet is PlayerActionPacket)
            {
                PlayerActionPacket playerAction = (PlayerActionPacket)packet;

                if (!playerAction.PlayerMustBeInRangeToReceive)
                {
                    return false;
                }

                AbsoluteEntityCell cell = new AbsoluteEntityCell(playerAction.ActionPosition);

                bool cellLoaded = false;

                for (int level = 0; level <= DESIRED_CELL_MIN_LOD_FOR_ACTIONS; level++)
                {
                    VisibleCell visibleCell = new VisibleCell(cell, level);

                    if (visibleCells.HasVisibleCell(visibleCell))
                    {
                        cellLoaded = true;
                        break;
                    }
                }

                if (!cellLoaded)
                {
                    Log.Debug("Action was deferred, cell not loaded (with required lod): " + cell);
                    AddPacketToDeferredMap(playerAction, cell);
                    return true;
                }
            }

            return false;
        }

        private void AddPacketToDeferredMap(PlayerActionPacket playerAction, AbsoluteEntityCell cell)
        {
            lock (deferredPacketsByAbsoluteCell)
            {
                if (!deferredPacketsByAbsoluteCell.ContainsKey(cell))
                {
                    deferredPacketsByAbsoluteCell.Add(cell, new Queue<Packet>());
                }

                deferredPacketsByAbsoluteCell[cell].Enqueue(playerAction);
            }
        }

        public void CellLoaded(VisibleCell visibleCell)
        {
            if (visibleCell.Level > DESIRED_CELL_MIN_LOD_FOR_ACTIONS)
            {
                return;
            }

            lock (deferredPacketsByAbsoluteCell)
            {
                Queue<Packet> deferredPackets;
                if (deferredPacketsByAbsoluteCell.TryGetValue(visibleCell.AbsoluteCellEntity, out deferredPackets))
                {
                    while (deferredPackets.Count > 0)
                    {
                        Log.Debug("Found deferred packet... adding it back with high priority.");
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
