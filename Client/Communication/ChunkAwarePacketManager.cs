using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication
{
    public class ChunkAwarePacketManager
    {
        private static int EXPIDITED_PACKET_PRIORITY = 999;
        private static int DEFAULT_PACKET_PRIORITY = 1;

        private Dictionary<Int3, Queue<Packet>> deferredPacketsByChunk;
        private Dictionary<Type, PriorityQueue<Packet>> receivedPacketsByType;
        private HashSet<Int3> loadedChunks;
        
        public ChunkAwarePacketManager()
        {
            this.deferredPacketsByChunk = new Dictionary<Int3, Queue<Packet>>();
            this.receivedPacketsByType = new Dictionary<Type, PriorityQueue<Packet>>();
            this.loadedChunks = new HashSet<Int3>();
        }

        public void PacketReceived(Packet packet)
        {
            lock (receivedPacketsByType)
            {
                if (!receivedPacketsByType.ContainsKey(packet.GetType()))
                {
                    receivedPacketsByType[packet.GetType()] = new PriorityQueue<Packet>();
                }

                receivedPacketsByType[packet.GetType()].Enqueue(DEFAULT_PACKET_PRIORITY, packet);
            }
        }
    
        public Queue<T> GetReceivedPacketsOfType<T>() where T : Packet
        {
            Queue<T> packetsOfType = new Queue<T>();

            lock (receivedPacketsByType)
            {
                if (receivedPacketsByType.ContainsKey(typeof(T)))
                {
                    while (receivedPacketsByType[typeof(T)].Count > 0)
                    {
                        Packet packet = receivedPacketsByType[typeof(T)].Dequeue();

                        Console.WriteLine("Processing packet: " + packet);

                        if (!PacketWasDeferred(packet))
                        {
                            packetsOfType.Enqueue((T)packet);
                        }
                    }
                }
            }

            return packetsOfType;
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

                Int3 actionsChunk = getActionsChunk(playerAction);
                bool actionOccuredInLoadedChunk = loadedChunks.Contains(actionsChunk);
                
                if(!actionOccuredInLoadedChunk)
                {
                    Console.WriteLine("Action was deferred");
                    addPacketToDeferredMap(playerAction, actionsChunk);
                    return true;
                }
            }

            return false;
        }

        private Int3 getActionsChunk(PlayerActionPacket playerAction)
        {
            int chunkSize = 16;
            Vector3 actionPosition = playerAction.ActionPosition;
            return new Int3((int)Math.Floor(actionPosition.X / chunkSize) * chunkSize,
                            (int)Math.Floor(actionPosition.Y / chunkSize) * chunkSize,
                            (int)Math.Floor(actionPosition.Z / chunkSize) * chunkSize);
        }

        private void addPacketToDeferredMap(PlayerActionPacket playerAction, Int3 chunk)
        {
            if (!deferredPacketsByChunk.ContainsKey(chunk))
            {
                deferredPacketsByChunk.Add(chunk, new Queue<Packet>());
            }

            deferredPacketsByChunk[chunk].Enqueue(playerAction);            
        }

        public void AddChunk(Int3 position)
        {
            loadedChunks.Add(position);

            if(deferredPacketsByChunk.ContainsKey(position))
            {               
                while(deferredPacketsByChunk[position].Count > 0)
                {
                    Console.WriteLine("Found deferred packet... adding it back with high priority.");
                    Packet packet = deferredPacketsByChunk[position].Dequeue();
                    receivedPacketsByType[packet.GetType()].Enqueue(EXPIDITED_PACKET_PRIORITY, packet);
                }
            }
        }

        public void RemoveChunk(Int3 position)
        {
            loadedChunks.Remove(position);
        }
    }
}
