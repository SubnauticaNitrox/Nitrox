﻿using System.Collections.Generic;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public class ChunkAwarePacketReceiver
    {
        private const int EXPIDITED_PACKET_PRIORITY = 999;
        private const int DEFAULT_PACKET_PRIORITY = 1;
        private const int DESIRED_CHUNK_MIN_LOD_FOR_ACTIONS = 1;

        private readonly Dictionary<Int3, Queue<Packet>> deferredPacketsByBatchId = new Dictionary<Int3, Queue<Packet>>();
        private readonly PriorityQueue<Packet> receivedPackets = new PriorityQueue<Packet>();
        private readonly LoadedChunks loadedChunks;

        public ChunkAwarePacketReceiver(LoadedChunks loadedChunks)
        {
            this.loadedChunks = loadedChunks;
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

                Int3 actionBatchId = LargeWorldStreamer.main.GetContainingBatch(playerAction.ActionPosition);

                if (!loadedChunks.HasChunkWithMinDesiredLevelOfDetail(actionBatchId, DESIRED_CHUNK_MIN_LOD_FOR_ACTIONS))
                {
                    Log.Debug("Action was deferred, batch not loaded (with required lod): " + actionBatchId);
                    AddPacketToDeferredMap(playerAction, actionBatchId);
                    return true;
                }
            }

            return false;
        }

        private void AddPacketToDeferredMap(PlayerActionPacket playerAction, Int3 batchId)
        {
            lock (deferredPacketsByBatchId)
            {
                if (!deferredPacketsByBatchId.ContainsKey(batchId))
                {
                    deferredPacketsByBatchId.Add(batchId, new Queue<Packet>());
                }

                deferredPacketsByBatchId[batchId].Enqueue(playerAction);
            }
        }

        public void ChunkLoaded(Chunk chunk)
        {
            if (chunk.Level > DESIRED_CHUNK_MIN_LOD_FOR_ACTIONS)
            {
                return;
            }

            lock (deferredPacketsByBatchId)
            {
                Queue<Packet> deferredPackets;
                if (deferredPacketsByBatchId.TryGetValue(chunk.BatchId, out deferredPackets))
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
    }
}
