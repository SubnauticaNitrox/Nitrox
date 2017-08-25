using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.Packets.WorldSending;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System.Collections.Generic;
using System.IO;

namespace NitroxServer.Communication.Packets.Processors.WorldSending
{
    public class AskBatchCellProcessor : AuthenticatedPacketProcessor<AskBatchCell>
    {
        private TcpServer tcpServer;
        private Dictionary<string, Queue<Chunk>> batchQueues = new Dictionary<string, Queue<Chunk>>();

        public static readonly string folderName = "DefaultSave"; //todo read from config

        public AskBatchCellProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void Process(AskBatchCell packet, Player player)
        {
            if (packet.FromChunkQueue)
            {
                ProcessFromQueue(player);
            }
            else
            {
                ProcessFromChunk(packet, player);
            }
        }

        public void ProcessFromQueue(Player player)
        {
            Queue<Chunk> batchQueue;
            if (!batchQueues.ContainsKey(player.Id))
            {
                batchQueue = new Queue<Chunk>();
                batchQueues.Add(player.Id, batchQueue);
                foreach (string file in Directory.GetFiles(folderName + "\\"))
                {
                    string[] chunkName = Path.GetFileNameWithoutExtension(file).Split('-');
                    Int3 chunkCoords = new Int3(int.Parse(chunkName[2]), int.Parse(chunkName[3]), int.Parse(chunkName[4]));
                    byte[] data = File.ReadAllBytes(file);
                    batchQueue.Enqueue(new Chunk(chunkCoords, data));
                }
            }
            else
            {
                batchQueue = batchQueues[player.Id];
            }
            RecieveBatchCell chunkPacket = new RecieveBatchCell(player.Id, batchQueue.Peek().chunkData, batchQueue.Peek().chunkCoord, batchQueue.Count > 1);
            batchQueue.Dequeue();
            tcpServer.SendPacketToPlayer(chunkPacket, player);
            if (batchQueue.Count == 0)
            {
                batchQueues.Remove(player.Id);
            }
        }

        public void ProcessFromChunk(AskBatchCell packet, Player player)
        {
            Int3 chunk = packet.ChunkLocation;
            string fileName = $"world-changes-{chunk.X}-{chunk.Y}-{chunk.Z}.dat";
            string file = Path.Combine(folderName, fileName);
            byte[] data = File.ReadAllBytes(file);

            RecieveBatchCell chunkPacket = new RecieveBatchCell(player.Id, data, packet.ChunkLocation, false);
            tcpServer.SendPacketToPlayer(chunkPacket, player);
        }
    }
}