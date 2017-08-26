using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Logger;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel.Packets.WorldSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RequestBatchCellProcessor : ClientPacketProcessor<ReceiveBatchCell>
    {
        public override void Process(ReceiveBatchCell chunk)
        {
            try
            {
                Multiplayer.Logic.Serializer.sendChunks = false;
                ClientLogger.IngameMessage($"Downloaded ({chunk.ChunkLocation.X}, {chunk.ChunkLocation.Y}, {chunk.ChunkLocation.Z})");
                Multiplayer.Logic.Serializer.LoadChunkSave(ApiHelper.Int3(chunk.ChunkLocation), chunk.ChunkChanges);
                if (chunk.StillMore)
                {
                    Multiplayer.PacketSender.Send(new AskBatchCell(chunk.PlayerId, true, new NitroxModel.DataStructures.Int3(0, 0, 0)));
                }
                else
                {
                    ClientLogger.IngameMessage("Chunks downloaded.");
                    Multiplayer.Logic.Serializer.sendChunks = true;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("Could not load chunk: " + ex.Message);
            }
        }
    }
}
