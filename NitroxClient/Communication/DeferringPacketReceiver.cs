using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public class DeferringPacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;
        private const int DESIRED_CELL_MIN_LOD_FOR_ACTIONS = 1;

        private readonly Dictionary<AbsoluteEntityCell, Queue<Packet>> deferredPacketsByAbsoluteCell = new Dictionary<AbsoluteEntityCell, Queue<Packet>>();
        private readonly NitroxModel.DataStructures.PriorityQueue<Packet> receivedPackets = new NitroxModel.DataStructures.PriorityQueue<Packet>();
        private readonly VisibleCells visibleCells;

        public DeferringPacketReceiver(VisibleCells visibleCells)
        {
            this.visibleCells = visibleCells;
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

                bool cellLoaded = false;

                for (int level = 0; level <= DESIRED_CELL_MIN_LOD_FOR_ACTIONS; level++)
                {
                    AbsoluteEntityCell cell = new AbsoluteEntityCell(playerAction.ActionPosition, level);

                    if (visibleCells.Contains(cell))
                    {
                        cellLoaded = true;
                        break;
                    }
                }

                if (!cellLoaded)
                {
                    // Hacky, just choose level 0 for now.
                    AbsoluteEntityCell cell = new AbsoluteEntityCell(playerAction.ActionPosition, 0);
                    Log.Debug($"Action {packet} was deferred, cell not loaded (with required lod): {cell}");
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

        public void CellLoaded(AbsoluteEntityCell visibleCell)
        {
            if (visibleCell.Level > DESIRED_CELL_MIN_LOD_FOR_ACTIONS)
            {
                return;
            }

            lock (deferredPacketsByAbsoluteCell)
            {
                Queue<Packet> deferredPackets;
                if (deferredPacketsByAbsoluteCell.TryGetValue(visibleCell, out deferredPackets))
                {
                    Log.Debug("Loaded {0}; found {1} deferred packet(s):{2}\nAdding it back with high priority.",
                        visibleCell,
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
