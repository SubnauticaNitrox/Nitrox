using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;
using System.IO;

namespace NitroxServer.Communication.Packets.Processors.WorldSending
{
    public class ChangeBatchCellProcessor : AuthenticatedPacketProcessor<ChangeBatchCell>
    {
        private TcpServer tcpServer;

        public ChangeBatchCellProcessor(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public override void Process(ChangeBatchCell packet, Player player)
        {
            string folderName = "DefaultSave";
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            Int3 location = packet.ChunkLocation;
            Console.WriteLine($"reading packet ({location.X}-{location.Y}-{location.Z})");
            byte[] data = packet.ChunkChanges;
            string filePath = $"{folderName}\\world-changes-{location.X}-{location.Y}-{location.Z}.dat";
            File.WriteAllBytes(filePath, data);
        }
    }
}
