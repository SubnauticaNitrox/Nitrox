using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;

namespace NitroxClient.Communication
{
    public class ChunkAwarePacketReceiver
    {
        private static readonly int EXPIDITED_PACKET_PRIORITY = 999;
        private static readonly int DEFAULT_PACKET_PRIORITY = 1;

        private Dictionary<Chunk, Queue<Packet>> deferredPacketsByChunk;
        private PriorityQueue<Packet> receivedPackets;
        private LoadedChunks loadedChunks;

        public ChunkAwarePacketReceiver(LoadedChunks loadedChunks)
        {
            this.deferredPacketsByChunk = new Dictionary<Chunk, Queue<Packet>>();
            this.receivedPackets = new PriorityQueue<Packet>();
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
                int levelOfDetailToShowAction = 1; //TODO: what is the correct value? cascaing down?

                Chunk chunk = new Chunk(actionBatchId, levelOfDetailToShowAction);

                if (!loadedChunks.Contains(chunk))
                {
                    Console.WriteLine("Action was deferred, chunk not loaded: " + chunk);
                    AddPacketToDeferredMap(playerAction, chunk);
                    return true;
                }
            }

            return false;
        }

        private void AddPacketToDeferredMap(PlayerActionPacket playerAction, Chunk chunk)
        {
            lock (deferredPacketsByChunk)
            {
                if (!deferredPacketsByChunk.ContainsKey(chunk))
                {
                    deferredPacketsByChunk.Add(chunk, new Queue<Packet>());
                }

                deferredPacketsByChunk[chunk].Enqueue(playerAction);
            }
        }

        public void ChunkLoaded(Chunk chunk)
        {
            lock (deferredPacketsByChunk)
            {
                if (deferredPacketsByChunk.ContainsKey(chunk))
                {
                    while (deferredPacketsByChunk[chunk].Count > 0)
                    {
                        Console.WriteLine("Found deferred packet... adding it back with high priority.");
                        Packet packet = deferredPacketsByChunk[chunk].Dequeue();
                        receivedPackets.Enqueue(EXPIDITED_PACKET_PRIORITY, packet);
                    }
                }
            }
        }
    }
}
