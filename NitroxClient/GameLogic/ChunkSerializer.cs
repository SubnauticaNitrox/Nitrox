using NitroxClient.Communication;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NitroxClient.GameLogic
{
    public class ChunkSerializer
    {
        private readonly string SERVER_LOCATION = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private PacketSender packetSender;
        public bool sendChunks = true;

        public ChunkSerializer(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SendChunkSave(Int3 chunkPos)
        {
            if (!sendChunks) return;
            ErrorMessage.AddMessage("Sending " + chunkPos.ToString());

            string originalPath = $"{PAXTerrainController.main.dataDirPath}\\CellsCache\\baked-batch-cells-{chunkPos.x}-{chunkPos.y}-{chunkPos.z}.bin";
            string currentPath = CellManager.GetCacheBatchCellsPath(LargeWorldStreamer.main.tmpPathPrefix, chunkPos);

            if (File.Exists(originalPath) && File.Exists(currentPath))
            {
                byte[] data;
                using (MemoryStream output = new MemoryStream())
                {
                    BsDiff.BinaryPatchUtility.Create(File.ReadAllBytes(originalPath), File.ReadAllBytes(currentPath), output);
                    data = output.ToArray();
                }
                packetSender.Send(new ChangeBatchCell(packetSender.PlayerId, data, ApiHelper.Int3(chunkPos)));
            }
        }

        public void LoadChunkSave(Int3 chunkPos, byte[] data)
        {
            string originalPath = $"{PAXTerrainController.main.dataDirPath}\\CellsCache\\baked-batch-cells-{chunkPos.x}-{chunkPos.y}-{chunkPos.z}.bin";
            string currentPath = CellManager.GetCacheBatchCellsPath(LargeWorldStreamer.main.tmpPathPrefix, chunkPos);
            new FileInfo(currentPath).Directory.Create(); //possibly redundant (filemode.create)
            if (File.Exists(originalPath))
            {
                //todo: reflection is slow but there is no game reload event so we
                //can't keep the batch2cells reference if we quit to menu and reload?
                CellManager cellManager = LargeWorldStreamer.main.cellManager;
                Dictionary<Int3, BatchCells> batch2cells = (Dictionary<Int3, BatchCells>)cellManager.ReflectionGet("batch2cells");
                //wipe cells if they are already loaded and reload them at the end
                LargeWorldStreamer.main.ReflectionCall("TryUnloadBatch", false, false, chunkPos);

                using (FileStream input = new FileStream(originalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream output = new FileStream(currentPath, FileMode.Create))
                    BsDiff.BinaryPatchUtility.Apply(input, () => new MemoryStream(data), output);

                LargeWorldStreamer.main.LoadBatch(chunkPos);
            }
        }
    }
}